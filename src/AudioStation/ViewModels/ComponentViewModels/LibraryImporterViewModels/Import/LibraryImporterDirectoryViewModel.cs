using SimpleWpf.UI.ViewModel.FileTreeView;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import
{
    /// <summary>
    /// PathViewModelUI provides the node VALUE for the recursive directory structure. The "Path" view model is essentially
    /// the container for this value.
    /// </summary>
    public class LibraryImporterDirectoryViewModel : FileTreeNodeViewModel
    {
        bool _inError;
        bool _areTagsDirty;
        bool _areAllMinimumImportsValid;

        public bool InError
        {
            get { return _inError; }
            set { this.RaiseAndSetIfChanged(ref _inError, value); }
        }
        public bool AreTagsDirty
        {
            get { return _areTagsDirty; }
            set { this.RaiseAndSetIfChanged(ref _areTagsDirty, value); }
        }
        public bool AreAllMinimumImportsValid
        {
            get { return _areAllMinimumImportsValid; }
            set { this.RaiseAndSetIfChanged(ref _areAllMinimumImportsValid, value); }
        }


        public LibraryImporterDirectoryViewModel(string fullDirectoryPath, string basePath, int directoryFileCount)
            : base(basePath, fullDirectoryPath, directoryFileCount)
        {
            this.IsLoaded = false;
        }

        public override string ToString()
        {
            return this.ShortPath;
        }
    }
}
