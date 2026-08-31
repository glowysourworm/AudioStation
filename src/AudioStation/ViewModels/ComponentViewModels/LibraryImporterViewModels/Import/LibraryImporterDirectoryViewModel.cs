using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import
{
    /// <summary>
    /// PathViewModelUI provides the node VALUE for the recursive directory structure. The "Path" view model is essentially
    /// the container for this value.
    /// </summary>
    public class LibraryImporterDirectoryViewModel : PathViewModel
    {
        bool _inError;
        bool _isLoaded;
        bool _areTagsDirty;
        bool _areAllMinimumImportsValid;

        public bool InError
        {
            get { return _inError; }
            set { SetValueOverride(ref _inError, value); }
        }

        /// <summary>
        /// Set to true when the directory has been iterated to add file instances to the tree
        /// </summary>
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetValueOverride(ref _isLoaded, value); }
        }
        public bool AreTagsDirty
        {
            get { return _areTagsDirty; }
            set { SetValueOverride(ref _areTagsDirty, value); }
        }
        public bool AreAllMinimumImportsValid
        {
            get { return _areAllMinimumImportsValid; }
            set { SetValueOverride(ref _areAllMinimumImportsValid, value); }
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
