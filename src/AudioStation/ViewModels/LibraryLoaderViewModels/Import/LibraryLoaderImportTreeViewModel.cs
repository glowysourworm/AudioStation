using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.LibraryLoaderViewModels.Import
{
    /// <summary>
    /// Class that represents a recursive directory tree structure based on SimpleWpf's RecursiveViewModel base class. Public
    /// properties:  Children, Parent, NodeValue (which is the LibraryloaderImportFileViewModel).
    /// </summary>
    public class LibraryLoaderImportTreeViewModel : DirectoryTreeViewModel<PathViewModelUI>
    {
        public LibraryLoaderImportTreeViewModel(PathViewModelUI nodeValue,
                                                RecursiveNodeViewModel<PathViewModelUI> parent = null) : base(nodeValue, parent)
        {
        }

        /// <summary>
        /// Provides a way to instantiate the tree. This is for direct children of this path's node.
        /// </summary>
        protected override RecursiveNodeViewModel<PathViewModelUI> Construct(PathViewModelUI nodeValue)
        {
            // FILE (OR) DIRECTORY
            //
            return new LibraryLoaderImportTreeViewModel(nodeValue, this);
        }
    }
}
