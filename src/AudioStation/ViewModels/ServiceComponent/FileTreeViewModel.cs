using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.ServiceComponent
{
    public class FileTreeViewModel : SimpleTreeViewModel
    {
        // These are forwarded directly from the UI
        IEnumerable<TreeViewNodeModelBase>? _selectedNodes;

        int _selectedCount;
        int _selectedFileCount;
        int _selectedDirectoryCount;
        int _totalFileCount;
        int _totalDirectoryCount;

        public IEnumerable<TreeViewNodeModelBase>? SelectedNodes
        {
            get { return _selectedNodes; }
            set { this.RaiseAndSetIfChanged(ref _selectedNodes, value); }
        }
        public int SelectedCount
        {
            get { return _selectedCount; }
            set { this.RaiseAndSetIfChanged(ref _selectedCount, value); }
        }
        public int SelectedFileCount
        {
            get { return _selectedFileCount; }
            set { this.RaiseAndSetIfChanged(ref _selectedFileCount, value); }
        }
        public int SelectedDirectoryCount
        {
            get { return _selectedDirectoryCount; }
            set { this.RaiseAndSetIfChanged(ref _selectedDirectoryCount, value); }
        }
        public int TotalFileCount
        {
            get { return _totalFileCount; }
            set { this.RaiseAndSetIfChanged(ref _totalFileCount, value); }
        }
        public int TotalDirectoryCount
        {
            get { return _totalDirectoryCount; }
            set { this.RaiseAndSetIfChanged(ref _totalDirectoryCount, value); }
        }

        public FileTreeViewModel()
        {
            _selectedCount = 0;
            _selectedFileCount = 0;
            _selectedDirectoryCount = 0;
            _totalDirectoryCount = 0;
            _totalFileCount = 0;

            _selectedNodes = null;

            this.TreeSelectionChangedEvent += OnTreeSelectionChangedEvent;
        }

        public override void EndUpdate()
        {
            base.EndUpdate();

            UpdateIndicators();
        }

        private void OnTreeSelectionChangedEvent(IEnumerable<TreeViewNodeModelBase> selectedNodes)
        {
            _selectedNodes = selectedNodes;

            UpdateIndicators();
        }
        public FileTreeNodeViewModel? GetNode(object key)
        {
            return base.GetNode<FileTreeNodeViewModel>(key);
        }
        public FileTreeNodeViewModel? GetNode(int recursionDepth, object key)
        {
            return base.GetNode<FileTreeNodeViewModel>(recursionDepth, key);
        }
        public bool Any(Func<FileTreeNodeViewModel, bool> predicate)
        {
            return base.Any<FileTreeNodeViewModel>(predicate);
        }
        public IEnumerable<FileTreeNodeViewModel> Where(Func<FileTreeNodeViewModel, bool> predicate)
        {
            return base.Where(predicate);
        }
        public FileTreeNodeViewModel? First(Func<FileTreeNodeViewModel, bool> predicate)
        {
            return base.First<FileTreeNodeViewModel>(predicate);
        }
        public int Count(Func<FileTreeNodeViewModel, bool> predicate)
        {
            return base.Count<FileTreeNodeViewModel>(predicate);
        }
        public void ForEach(Action<FileTreeNodeViewModel> action)
        {
            base.ForEach<FileTreeNodeViewModel>(action);
        }
        public new IEnumerable<FileTreeNodeViewModel> GetBranch(TreeViewNodeModelBase node, bool includeDescendants = false)
        {
            return base.GetBranch(node, includeDescendants).Cast<FileTreeNodeViewModel>();
        }

        public List<FileTreeNodeViewModel> RecursiveToList()
        {
            var allNodes = new List<FileTreeNodeViewModel>();

            ForEach(allNodes.Add);

            return allNodes;
        }
        public List<T> RecursiveToList<T>() where T : TreeViewNodeModelBase
        {
            var allNodes = new List<T>();

            ForEach<T>(allNodes.Add);

            return allNodes;
        }

        private void UpdateIndicators()
        {
            _selectedCount = 0;
            _selectedFileCount = 0;
            _selectedDirectoryCount = 0;
            _totalDirectoryCount = 0;
            _totalFileCount = 0;

            this.ForEach(node =>
            {
                if (node.IsDirectory)
                {
                    _totalDirectoryCount++;

                    if (node.IsSelected)
                        _selectedDirectoryCount++;
                }
                else
                {
                    _totalFileCount++;

                    if (node.IsSelected)
                        _selectedFileCount++;
                }

                if (node.IsSelected)
                    _selectedCount++;
            });
        }
    }
}
