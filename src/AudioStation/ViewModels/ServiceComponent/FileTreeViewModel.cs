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

        public bool RecursiveAny(Func<FileTreeNodeViewModel, bool> predicate)
        {
            return base.RecursiveAny<FileTreeNodeViewModel>(predicate);
        }
        public IEnumerable<FileTreeNodeViewModel> RecursiveWhere(Func<FileTreeNodeViewModel, bool> predicate)
        {
            return base.RecursiveWhere(predicate);
        }
        public FileTreeNodeViewModel? RecursiveFirst(Func<FileTreeNodeViewModel, bool> predicate)
        {
            return base.RecursiveFirst<FileTreeNodeViewModel>(predicate);
        }
        public int RecursiveCount(Func<FileTreeNodeViewModel, bool> predicate)
        {
            return base.RecursiveCount<FileTreeNodeViewModel>(predicate);
        }
        public void RecursiveForEach(Action<FileTreeNodeViewModel> action)
        {
            base.RecursiveForEach<FileTreeNodeViewModel>(action);
        }
        public List<FileTreeNodeViewModel> RecursiveToList()
        {
            var allNodes = new List<FileTreeNodeViewModel>();

            RecursiveForEach(allNodes.Add);

            return allNodes;
        }
        public List<T> RecursiveToList<T>() where T : TreeViewNodeModelBase
        {
            var allNodes = new List<T>();

            RecursiveForEach<T>(allNodes.Add);

            return allNodes;
        }

        private void UpdateIndicators()
        {
            _selectedCount = 0;
            _selectedFileCount = 0;
            _selectedDirectoryCount = 0;
            _totalDirectoryCount = 0;
            _totalFileCount = 0;

            this.RecursiveForEach<FileTreeNodeViewModel>(node =>
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
