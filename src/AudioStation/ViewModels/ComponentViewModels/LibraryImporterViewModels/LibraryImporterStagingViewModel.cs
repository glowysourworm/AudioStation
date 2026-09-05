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
using SimpleWpf.UI.ViewModel.TreeView;
using System.ComponentModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    public class LibraryImporterStagingViewModel : ComponentViewModelBase
    {
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

        public LibraryImporterStagingViewModel(LibraryImporterConfigurationViewModel options)
        {
            this.ImportOptions = options;
            this.StagedFiles = new ObservableCollection<LibraryImporterFileViewModel>();

            this.StageCommand = new SimpleCommand(Stage, CanStage);
            this.UnstageCommand = new SimpleCommand(Unstage, CanUnstage);
        }

        public void Stage()
        {
            // Selected Nodes
            var selectedNodes = this.ImportDirectory.GetSelection(true);

            // -> Select any selected files or any files in a sub-directory recursively
            foreach (var node in selectedNodes)
            {
                // Directory:  Recurse down this sub-tree and add files only
                if (node.NodeValue.IsDirectory)
                {
                    node.RecurseForEach(subNode =>
                    {
                        if (!subNode.NodeValue.IsDirectory && !this.StagedFiles.Any(x => x.FullPath == subNode.NodeValue.FullPath))
                            this.StagedFiles.Add(new LibraryImporterFileViewModel(subNode.NodeValue.FullPath, subNode.NodeValue.BaseDirectory, this.ImportOptions.ImportType));
                    });
                }

                // Other Files
                else if (!this.StagedFiles.Any(x => x.FullPath == node.NodeValue.FullPath))
                    this.StagedFiles.Add(new LibraryImporterFileViewModel(node.NodeValue.FullPath, node.NodeValue.BaseDirectory, this.ImportOptions.ImportType));
            }
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
                this.ImportDirectory.NodeValue.BaseDirectory != this.ImportOptions.ImportDirectory.Directory)
            {
                var libraryLoaderService = IocContainer.Get<ILibraryLoaderService>();
                var directory = (this.ImportOptions.ImportType == Core.Model.LibraryImportType.Migration) ? this.ImportOptions.MigrationSourceDirectory :
                                                                                                            this.ImportOptions.ImportDirectory.Directory;

                // Unhook
                this.ImportDirectory?.ItemPropertyChangedTreeEvent -= OnImportTreePropertyChanged;

                this.ImportDirectory = libraryLoaderService.InitializeImporterTree(directory, searchPattern, this.ImportOptions);

                // Hook
                this.ImportDirectory.ItemPropertyChangedTreeEvent += OnImportTreePropertyChanged;
            }
        }

        private void OnImportTreePropertyChanged(TreeViewModelBase<FileTreeNodeViewModel> treeSender, FileTreeNodeViewModel item, PropertyChangedEventArgs eventArgs)
        {
            this.StageCommand.RaiseCanExecuteChanged();
            this.UnstageCommand.RaiseCanExecuteChanged();
        }
    }
}
