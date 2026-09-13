using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.UI.ViewModel.TreeView;
using SimpleWpf.UI.ViewModel.TreeView.Interface;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterStagingWorkflowViewModel : ComponentPartViewModelBase
    {
        private IAudioStationDbClient _audioStationDbClient;
        private ITagCache _tagCache;
        private LibraryImporterWorkflowViewModel _workflow;

        SimpleCommand _stageCommand;
        SimpleCommand _unstageCommand;

        // Import Source Directory
        //
        FileTreeViewModel _importDirectory;

        // Staged Files:  These will keep changes to the tag in memory until the tag data is saved (to
        //                the same file in the source direcotry. An "import" is complete when the file 
        //                finished - with the bare minimum tag data - and moved into the library's 
        //                directory structure.
        //
        NotifyingObservableCollection<LibraryImporterFileViewModel> _stagedFiles;

        int _libraryConflictCount;
        int _stagedSelectedCount;

        // Import Directory:  Selected file count
        //
        int _totalFileCount;
        int _totalDirectoryCount;
        int _selectedFileCount;

        public LibraryImporterWorkflowViewModel Workflow
        {
            get { return _workflow; }
            set { this.RaiseAndSetIfChanged(ref _workflow, value); }
        }
        public FileTreeViewModel ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public NotifyingObservableCollection<LibraryImporterFileViewModel> StagedFiles
        {
            get { return _stagedFiles; }
            set { this.RaiseAndSetIfChanged(ref _stagedFiles, value); }
        }
        public int LibraryConflictCount
        {
            get { return _libraryConflictCount; }
            set { this.RaiseAndSetIfChanged(ref _libraryConflictCount, value); }
        }
        public int StagedSelectedCount
        {
            get { return _stagedSelectedCount; }
            set { this.RaiseAndSetIfChanged(ref _stagedSelectedCount, value); }
        }
        public int TotalFileCount
        {
            get { return _totalFileCount; }
            set { this.RaiseAndSetIfChanged(ref _totalFileCount, value); }
        }
        public int TotalDirectoryCount
        {
            get { return _totalDirectoryCount; }
            set { this.RaiseAndSetIfChanged(ref _totalDirectoryCount, value); }
        }
        public int SelectedFileCount
        {
            get { return _selectedFileCount; }
            set { this.RaiseAndSetIfChanged(ref _selectedFileCount, value); }
        }

        public SimpleCommand StageCommand
        {
            get { return _stageCommand; }
            set { this.RaiseAndSetIfChanged(ref _stageCommand, value); }
        }
        public SimpleCommand UnstageCommand
        {
            get { return _unstageCommand; }
            set { this.RaiseAndSetIfChanged(ref _unstageCommand, value); }
        }

        public LibraryImporterStagingWorkflowViewModel(IDialogController dialogController, LibraryImporterWorkflowViewModel workflow)
            : base("Library Importer (staging)")
        {
            this.Workflow = workflow;
            this.StagedFiles = new NotifyingObservableCollection<LibraryImporterFileViewModel>();
            this.StagedFiles.ItemPropertyChanged += StagedFiles_ItemPropertyChanged;

            this.StageCommand = new SimpleCommand(() => Stage(dialogController), CanStage);
            this.UnstageCommand = new SimpleCommand(Unstage, CanUnstage);
        }

        public void Stage(IDialogController dialogController)
        {
            dialogController.ShowLoading("Staging Files", ExecuteWork);
        }
        public void Unstage()
        {
            this.StagedFiles.Remove(x => x.IsSelected);
        }
        public bool CanStage()
        {
            // This needs to be completed... (Return bools from Initialize  and Load)
            if (this.ImportDirectory == null)
                return false;

            return this.ImportDirectory.RecursiveCount(x => x.IsSelected) > 0;
        }
        public bool CanUnstage()
        {
            return this.StagedFiles.Any(x => x.IsSelected);
        }

        public override bool CanExecute()
        {
            return CanStage();
        }

        protected override void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            _audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();
            _tagCache = audioStationController.ServiceController.GetCache<ITagCache>();

            // TODO: Put this somewhere and verify convertible files on startup
            var searchPattern = "*.mp3";

            // Import Directory:  1) Not Initialized; or 2) A different directory
            //
            if (this.ImportDirectory == null ||
                this.ImportDirectory.GetNodeValue().BaseDirectory != this.Workflow.Configuration.ImportDirectory.Directory)
            {
                var libraryLoaderService = IocContainer.Get<ILibraryLoaderService>();
                var directory = (this.Workflow.Configuration.ImportDirectory.ImportType == Core.Model.LibraryImportType.Migration) ? this.Workflow.Configuration.MigrationSourceDirectory :
                                                                                                                            this.Workflow.Configuration.ImportDirectory.Directory;
                // Clear Staged
                this.StagedFiles.Clear();

                // Unhook
                this.ImportDirectory?.ItemPropertyChangedTreeEvent -= OnImportTreePropertyChanged;

                this.ImportDirectory = libraryLoaderService.InitializeImporterTree(directory, searchPattern, this.Workflow.Configuration, progressHandler);

                this.TotalFileCount = this.ImportDirectory.RecursiveCount(x => !x.CanHaveChildren);
                this.TotalDirectoryCount = this.ImportDirectory.RecursiveCount(x => x.CanHaveChildren);

                // Hook
                this.ImportDirectory.ItemPropertyChangedTreeEvent += OnImportTreePropertyChanged;
            }
        }
        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Library Files
            var libraryFiles = _audioStationDbClient.GetEntities<FileReference>().ToDictionary(x => x.FileName);

            // (performance) Staged File Dictionary 
            var stagedFiles = new Dictionary<string, LibraryImporterFileViewModel>();

            // Initialize with any staged files
            foreach (var file in this.StagedFiles)
            {
                if (!file.IsDirectory && !stagedFiles.ContainsKey(file.FullPath))
                    stagedFiles.Add(file.FullPath, file);
            }

            // Selected Nodes
            var selectedNodes = this.ImportDirectory.GetSelection(true).ToList();
            var counter = 0;

            // -> Select any selected files or any files in a sub-directory recursively
            foreach (var nodeBase in selectedNodes)
            {
                progressHandler(selectedNodes.Count, counter++, 0, "Loading:  " + nodeBase.NodeValue.DisplayName);

                var node = nodeBase.GetNodeValue();
                var subCounter = 0;

                // Directory:  Recurse down this sub-tree and add files only
                if (node.IsDirectory)
                {
                    nodeBase.RecurseForEach(subNodeBase =>
                    {
                        progressHandler(nodeBase.Children.Count, subCounter++, 0, "Loading:  " + subNodeBase.NodeValue.DisplayName);

                        var subNode = subNodeBase.NodeValue as FileTreeNodeViewModel;

                        if (!subNode.IsDirectory && !stagedFiles.ContainsKey(subNode.FullPath))
                        {
                            var file = new LibraryImporterFileViewModel(subNode.FullPath, subNode.BaseDirectory);

                            file.TagClean = _tagCache.Get(subNode.FullPath);
                            file.TagDirty = _tagCache.GetCopy(subNode.FullPath);

                            // Check For Library Conflict
                            //
                            file.LibraryConflict = libraryFiles.ContainsKey(file.FullPath);
                            file.FileConflict = false;                                          // Calculate migration path

                            stagedFiles.Add(file.FullPath, file);
                            this.StagedFiles.Add(file);
                        }

                    });
                }

                // Other Files
                else if (!stagedFiles.ContainsKey(node.FullPath))
                {
                    var stagedFile = new LibraryImporterFileViewModel(node.FullPath, node.BaseDirectory);

                    stagedFile.TagClean = _tagCache.Get(node.FullPath);
                    stagedFile.TagDirty = _tagCache.GetCopy(node.FullPath);

                    // Check For Library Conflict
                    //
                    stagedFile.LibraryConflict = libraryFiles.ContainsKey(stagedFile.FullPath);
                    stagedFile.FileConflict = false;                                                  // Calculate migration path

                    stagedFiles.Add(node.FullPath, stagedFile);
                    this.StagedFiles.Add(stagedFile);
                }
            }
        }
        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
        private void OnImportTreePropertyChanged(TreeViewModelBase treeSender, ITreeViewNode item, PropertyChangedEventArgs eventArgs)
        {
            this.StageCommand.RaiseCanExecuteChanged();
            this.UnstageCommand.RaiseCanExecuteChanged();
        }
        private void StagedFiles_ItemPropertyChanged(LibraryImporterFileViewModel item, PropertyChangedEventArgs propertyArgs)
        {
            if (propertyArgs.PropertyName == "IsSelected")
            {
                this.StageCommand.RaiseCanExecuteChanged();
                this.UnstageCommand.RaiseCanExecuteChanged();

                // Selection Counts (PERFORMANCE LAG)
                //this.StagedSelectedCount = this.StagedFiles.Count(x => x.IsSelected);
                //this.LibraryConflictCount = this.StagedFiles.Count(x => x.LibraryConflict);
            }
        }
    }
}
