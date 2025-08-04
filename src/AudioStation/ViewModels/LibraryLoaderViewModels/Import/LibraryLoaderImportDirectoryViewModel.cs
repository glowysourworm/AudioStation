using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.LibraryLoaderViewModels.Import
{
    /// <summary>
    /// PathViewModelUI provides the node VALUE for the recursive directory structure. The "Path" view model is essentially
    /// the container for this value.
    /// </summary>
    public class LibraryLoaderImportDirectoryViewModel : PathViewModel
    {
        bool _inError;
        bool _areTagsDirty;
        bool _areAllMinimumImportsValid;

        public bool InError
        {
            get { return _inError; }
            set { SetValueOverride(ref _inError, value); }
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

        public LibraryLoaderImportDirectoryViewModel(string fullDirectoryPath,
                                                     LibraryLoaderImportOptionsViewModel options)
            : base(options.SourceFolder, fullDirectoryPath)
        {
        }

        public override string ToString()
        {
            return this.ShortPath;
        }
    }
}
