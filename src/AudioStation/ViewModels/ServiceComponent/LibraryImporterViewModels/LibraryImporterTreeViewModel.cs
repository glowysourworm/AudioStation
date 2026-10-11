using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    public class LibraryImporterTreeViewModel : FileTreeViewModel
    {
        public LibraryImporterTreeViewModel()
        {
        }

        public void Remove(Func<TreeViewNodeModelBase, bool> predicate)
        {
            var nodes = this.Where(predicate);

            BeginUpdate();

            // Have to check each time because of branches that might have been 
            // related
            //
            foreach (var node in nodes)
            {
                if (Contains(node))
                    Remove(node);
            }

            EndUpdate();
        }
        public new TreeViewNodeModelBase? GetNode(object key)
        {
            return base.GetNode<TreeViewNodeModelBase>(key);
        }
        public new TreeViewNodeModelBase? GetNode(int recursionDepth, object key)
        {
            return base.GetNode<TreeViewNodeModelBase>(recursionDepth, key);
        }
        public bool Any(Func<TreeViewNodeModelBase, bool> predicate)
        {
            return base.Any<TreeViewNodeModelBase>(predicate);
        }
        public bool AnyFiles(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            // The file type should be at the leaf recursion depth
            return base.Any<TreeViewNodeModelBase>(item =>
            {
                if (item is LibraryImporterFileTreeNodeViewModel)
                    return predicate(item as LibraryImporterFileTreeNodeViewModel);

                else
                    return false;
            });
        }
        public IEnumerable<TreeViewNodeModelBase> Where(Func<TreeViewNodeModelBase, bool> predicate)
        {
            return base.Where(predicate);
        }
        public IEnumerable<LibraryImporterFileTreeNodeViewModel> WhereFiles(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.Where(item =>
            {
                return (item is LibraryImporterFileTreeNodeViewModel) && predicate(item as LibraryImporterFileTreeNodeViewModel);

            }).Cast<LibraryImporterFileTreeNodeViewModel>();
        }
        public TreeViewNodeModelBase? First(Func<TreeViewNodeModelBase, bool> predicate)
        {
            return base.First<TreeViewNodeModelBase>(predicate);
        }
        public int Count(Func<TreeViewNodeModelBase, bool> predicate)
        {
            return base.Count<TreeViewNodeModelBase>(predicate);
        }
        public int CountFiles(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            // The file type should be at the leaf recursion depth
            return base.Count<TreeViewNodeModelBase>(item =>
            {
                if (item is LibraryImporterFileTreeNodeViewModel)
                    return predicate(item as LibraryImporterFileTreeNodeViewModel);

                else
                    return false;
            });
        }
        public void ForEach(Action<TreeViewNodeModelBase> action)
        {
            base.ForEach<TreeViewNodeModelBase>(action);
        }
    }
}