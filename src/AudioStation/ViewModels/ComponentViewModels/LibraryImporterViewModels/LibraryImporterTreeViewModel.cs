using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.UI.ViewModel.TreeView;
using SimpleWpf.UI.ViewModel.TreeView.Interface;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    /// <summary>
    /// Class that represents a recursive directory tree structure based on SimpleWpf's RecursiveViewModel base class. Public
    /// properties:  Children, Parent, NodeValue (which is the LibraryloaderImportFileViewModel).
    /// </summary>
    public class LibraryImporterTreeViewModel : FileTreeViewModel
    {
        public LibraryImporterTreeViewModel(FileTreeNodeViewModel nodeValue, string searchPattern, LibraryImporterTreeViewModel parent = null)
            : base(searchPattern, nodeValue, parent)
        {
        }
        protected override TreeViewModelBase Construct(ITreeViewNode nodeValue)
        {
            return new LibraryImporterTreeViewModel(nodeValue as FileTreeNodeViewModel, this.SearchPattern, this);
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

        public override string ToString()
        {
            return this.NodeValue.ToString();
        }
    }
}
