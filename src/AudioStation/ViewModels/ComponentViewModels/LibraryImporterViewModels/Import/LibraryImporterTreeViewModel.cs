using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import
{
    /// <summary>
    /// Class that represents a recursive directory tree structure based on SimpleWpf's RecursiveViewModel base class. Public
    /// properties:  Children, Parent, NodeValue (which is the LibraryloaderImportFileViewModel).
    /// </summary>
    public class LibraryImporterTreeViewModel : RecursiveDispatcherViewModel<PathViewModel>
    {
        public LibraryImporterTreeViewModel(PathViewModel nodeValue, RecursiveDispatcherViewModel<PathViewModel> parent = null)
            : base(nodeValue, parent)
        {
        }

        protected override RecursiveDispatcherViewModel<PathViewModel> Construct(PathViewModel nodeValue)
        {
            return new LibraryImporterTreeViewModel(nodeValue, this);
        }

        public bool HasSelectedParent()
        {
            if (this.Parent != null)
            {
                if (this.Parent.NodeValue.IsSelected)
                    return true;

                else
                    return (this.Parent as LibraryImporterTreeViewModel).HasSelectedParent();
            }

            return false;
        }
    }
}
