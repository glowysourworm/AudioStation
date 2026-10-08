namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    public class LibraryImporterFileTreeViewModel : FileTreeViewModel
    {
        public LibraryImporterFileTreeViewModel()
        {
        }

        public void Remove(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
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
        public new LibraryImporterFileTreeNodeViewModel? GetNode(object key)
        {
            return base.GetNode<LibraryImporterFileTreeNodeViewModel>(key);
        }
        public new LibraryImporterFileTreeNodeViewModel? GetNode(int recursionDepth, object key)
        {
            return base.GetNode<LibraryImporterFileTreeNodeViewModel>(recursionDepth, key);
        }
        public bool Any(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.Any<LibraryImporterFileTreeNodeViewModel>(predicate);
        }
        public IEnumerable<LibraryImporterFileTreeNodeViewModel> Where(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.Where(predicate);
        }
        public LibraryImporterFileTreeNodeViewModel? First(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.First<LibraryImporterFileTreeNodeViewModel>(predicate);
        }
        public int Count(Func<LibraryImporterFileTreeNodeViewModel, bool> predicate)
        {
            return base.Count<LibraryImporterFileTreeNodeViewModel>(predicate);
        }
        public void ForEach(Action<LibraryImporterFileTreeNodeViewModel> action)
        {
            base.ForEach<LibraryImporterFileTreeNodeViewModel>(action);
        }
        public IEnumerable<LibraryImporterFileTreeNodeViewModel> GetBranch(LibraryImporterFileTreeNodeViewModel node, bool includeDescendants = false)
        {
            return base.GetBranch(node, includeDescendants).Cast<LibraryImporterFileTreeNodeViewModel>();
        }
    }
}