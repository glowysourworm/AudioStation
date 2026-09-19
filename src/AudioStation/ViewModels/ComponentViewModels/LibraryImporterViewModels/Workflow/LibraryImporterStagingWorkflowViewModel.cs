using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
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

        // Workflow Configuration
        private LibraryImporterConfigurationViewModel _workflowConfiguration;

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
        KeyedObservableCollection<string, LibraryImporterFileViewModel> _stagedFiles;

        int _libraryConflictCount;
        int _stagedSelectedCount;

        // Import Directory:  Selected file count
        //
        int _totalFileCount;
        int _totalDirectoryCount;
        int _selectedFileCount;

        public FileTreeViewModel ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public KeyedObservableCollection<string, LibraryImporterFileViewModel> StagedFiles
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

        public LibraryImporterStagingWorkflowViewModel(IDialogController dialogController, LibraryImporterConfigurationViewModel workflowConfiguration)
            : base("Library Importer (staging)")
        {
            _workflowConfiguration = workflowConfiguration;

            this.StagedFiles = new KeyedObservableCollection<string, LibraryImporterFileViewModel>();
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

            var audioConverter = IocContainer.Get<IAudioConverter>();

            // Multiple File Search
            var searchPattern = audioConverter.GetSupportedFormatExtensions()
                                              .Select(x => "*" + x)
                                              .ToArray();

            // Import Directory:  1) Not Initialized; or 2) A different directory
            //
            if (this.ImportDirectory == null ||
                this.ImportDirectory.GetNodeValue().BaseDirectory != _workflowConfiguration.ImportDirectory.Directory)
            {
                var libraryLoaderService = IocContainer.Get<ILibraryLoaderService>();
                var directory = (_workflowConfiguration.ImportType == Core.Model.LibraryImportType.Migration) ? _workflowConfiguration.MigrationSourceDirectory :
                                                                                                                _workflowConfiguration.ImportDirectory.Directory;
                // Clear Staged
                this.StagedFiles.Clear();

                // Unhook
                this.ImportDirectory?.ItemPropertyChangedTreeEvent -= OnImportTreePropertyChanged;

                this.ImportDirectory = libraryLoaderService.InitializeImporterTree(directory, _workflowConfiguration, progressHandler, searchPattern);

                this.TotalFileCount = this.ImportDirectory.RecursiveCount(x => !x.CanHaveChildren);
                this.TotalDirectoryCount = this.ImportDirectory.RecursiveCount(x => x.CanHaveChildren);

                // Hook
                this.ImportDirectory.ItemPropertyChangedTreeEvent += OnImportTreePropertyChanged;
            }
        }
        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Block Events
            this.StagedFiles.BeginUpdate();

            progressHandler(1, 1, 1, 0, "Loading Library Files");

            // Library Files
            var libraryFiles = _audioStationDbClient.GetEntities<FileReference>().ToDictionary(x => x.FileName);

            var selectedFileCount = this.ImportDirectory.GetSelectedFileCount();
            var counter = 0;

            // -> Select any selected files or any files in a sub-directory recursively
            this.ImportDirectory.RecurseForEach(treeBase =>
            {
                // Selection Only
                if (!treeBase.NodeValue.IsSelected)
                    return;

                var subTree = treeBase as FileTreeViewModel;
                var subNode = subTree.GetNodeValue();

                // Careful to avoid other files that have been staged
                if (!subNode.IsDirectory && !this.StagedFiles.ContainsKey(subNode.FullPath))
                {
                    // Progress
                    progressHandler(1, 1, selectedFileCount, counter++, "Loading:  " + treeBase.NodeValue.DisplayName);

                    var stagedFile = new LibraryImporterFileViewModel(subNode.FullPath, subNode.BaseDirectory, _workflowConfiguration);

                    //stagedFile.TagClean = _tagCache.Get(subNode.FullPath);
                    //stagedFile.TagDirty = _tagCache.GetCopy(node.FullPath);

                    // Check For Library Conflict
                    //
                    stagedFile.LibraryConflict = libraryFiles.ContainsKey(stagedFile.FullPath);
                    stagedFile.FileConflict = false;                                                  // Calculate migration path

                    this.StagedFiles.Add(stagedFile.FullPath, stagedFile);
                }
            });

            this.StagedFiles.EndUpdate(true);
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
