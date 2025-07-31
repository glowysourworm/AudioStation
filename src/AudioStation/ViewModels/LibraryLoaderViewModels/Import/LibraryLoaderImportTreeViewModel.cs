using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.LibraryLoaderViewModels.Import
{
    /// <summary>
    /// Class that represents a recursive directory tree structure based on SimpleWpf's RecursiveViewModel base class. Public
    /// properties:  Children, Parent, NodeValue (which is the LibraryloaderImportFileViewModel).
    /// </summary>
    public class LibraryLoaderImportTreeViewModel : RecursiveDispatcherViewModel<PathViewModel>
    {
        public LibraryLoaderImportTreeViewModel(PathViewModel nodeValue, RecursiveDispatcherViewModel<PathViewModel> parent = null) 
            : base(nodeValue, parent)
        {
        }

        protected override RecursiveDispatcherViewModel<PathViewModel> Construct(PathViewModel nodeValue)
        {
            return new LibraryLoaderImportTreeViewModel(nodeValue, this);
        }
    }
}
