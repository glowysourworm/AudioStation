using System.Collections.ObjectModel;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.Extensions.Collection;
using System.ComponentModel;
using SimpleWpf.UI.ViewModel.TreeView;
using SimpleWpf.UI.ViewModel.TreeView.Interface;
using SimpleWpf.IocFramework.EventAggregation;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    public class LibraryImporterStagingViewModel : ComponentViewModelBase
    {
        private readonly IIocEventAggregator _eventAggregator;
        private LibraryImporterConfigurationViewModel _importOptions;

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
        ObservableCollection<LibraryImporterFileViewModel> _stagedFiles;

        public LibraryImporterConfigurationViewModel ImportOptions
        {
            get { return _importOptions; }
            set { this.RaiseAndSetIfChanged(ref _importOptions, value); }
        }
        public FileTreeViewModel ImportDirectory
        {
            get { return _importDirectory; }
            set { this.RaiseAndSetIfChanged(ref _importDirectory, value); }
        }
        public ObservableCollection<LibraryImporterFileViewModel> StagedFiles
        {
            get { return _stagedFiles; }
            set { this.RaiseAndSetIfChanged(ref _stagedFiles, value); }
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

        public LibraryImporterStagingViewModel(IIocEventAggregator eventAggregator, LibraryImporterConfigurationViewModel options)
            : base("Library Importer (staging)")
        {
            _eventAggregator = eventAggregator;

            this.ImportOptions = options;
            this.StagedFiles = new ObservableCollection<LibraryImporterFileViewModel>();

            this.StageCommand = new SimpleCommand(Stage, CanStage);
            this.UnstageCommand = new SimpleCommand(Unstage, CanUnstage);
        }

        public void Stage()
        {
            var stagedFiles = new Dictionary<string, LibraryImporterFileViewModel>();

            var eventData = DialogEventData.ShowLoadingWithProgress("Staging Files");
            var dialogViewModel = eventData.DataContext as DialogLoadingViewModel;

            _eventAggregator.GetEvent<DialogEvent>().Publish(eventData);

            // Selected Nodes
            var selectedNodes = this.ImportDirectory.GetSelection(true).ToList();
            var counter = 0;

            // -> Select any selected files or any files in a sub-directory recursively
            foreach (var nodeBase in selectedNodes)
            {
                dialogViewModel.ShowProgressBar = true;
                dialogViewModel.Progress = Math.Clamp((counter++ / (double)selectedNodes.Count), 0, 1);

                var node = nodeBase.GetNodeValue();
                var subCounter = 0;

                // Directory:  Recurse down this sub-tree and add files only
                if (node.IsDirectory)
                {
                    nodeBase.RecurseForEach(subNodeBase =>
                    {
                        dialogViewModel.Progress = Math.Clamp((subCounter++ / (double)nodeBase.Children.Count), 0, 1);

                        var subNode = subNodeBase.NodeValue as FileTreeNodeViewModel;

                        if (!subNode.IsDirectory && !stagedFiles.ContainsKey(subNode.FullPath))
                        {
                            var file = new LibraryImporterFileViewModel(subNode.FullPath, subNode.BaseDirectory, this.ImportOptions.ImportType);

                            stagedFiles.Add(file.FullPath, file);
                            this.StagedFiles.Add(file);
                        }

                    });
                }

                // Other Files
                else if (!stagedFiles.ContainsKey(node.FullPath))
                {
                    var stagedFile = new LibraryImporterFileViewModel(node.FullPath, node.BaseDirectory, this.ImportOptions.ImportType);

                    stagedFiles.Add(node.FullPath, stagedFile);
                    this.StagedFiles.Add(stagedFile);
                }
            }

            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }
        public void Unstage()
        {
            this.StagedFiles.Remove(x => x.IsSelected);
        }
        public bool CanStage()
        {
            return this.ImportDirectory.RecursiveCount(x => x.IsSelected) > 0;
        }
        public bool CanUnstage()
        {
            return this.StagedFiles.Any(x => x.IsSelected);
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationViewModelController viewModelController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IComponentViewModelLoader viewModelLoader, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.ImportOptions == null)
                return;

            // TODO: Put this somewhere and verify convertible files on startup
            var searchPattern = "*.mp3";

            // Import Directory:  1) Not Initialized; or 2) A different directory
            //
            if (this.ImportDirectory == null ||
                this.ImportDirectory.GetNodeValue().BaseDirectory != this.ImportOptions.ImportDirectory.Directory)
            {
                var libraryLoaderService = IocContainer.Get<ILibraryLoaderService>();
                var directory = (this.ImportOptions.ImportType == Core.Model.LibraryImportType.Migration) ? this.ImportOptions.MigrationSourceDirectory :
                                                                                                            this.ImportOptions.ImportDirectory.Directory;

                // Unhook
                this.ImportDirectory?.ItemPropertyChangedTreeEvent -= OnImportTreePropertyChanged;

                this.ImportDirectory = libraryLoaderService.InitializeImporterTree(directory, searchPattern, this.ImportOptions, progressHandler);

                // Hook
                this.ImportDirectory.ItemPropertyChangedTreeEvent += OnImportTreePropertyChanged;
            }
        }

        private void OnImportTreePropertyChanged(TreeViewModelBase treeSender, ITreeViewNode item, PropertyChangedEventArgs eventArgs)
        {
            this.StageCommand.RaiseCanExecuteChanged();
            this.UnstageCommand.RaiseCanExecuteChanged();
        }
    }
}
