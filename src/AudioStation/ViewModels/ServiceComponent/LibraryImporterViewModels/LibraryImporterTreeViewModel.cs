using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels
{
    /// <summary>
    /// Class for handling the hybrid tree for import files:  Genre -> Album -> Artist -> Media -> Track (potentially)
    /// </summary>
    public class LibraryImporterTreeViewModel : SimpleTreeViewModel
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

        public LibraryImporterTreeViewModel()
            : base(new SimpleTreeCollectionViewModel(SimpleTreeViewBranchingStrategy.CommonParent))
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
            return base.Where<TreeViewNodeModelBase>(item =>
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
        private void OnTreeSelectionChangedEvent(IEnumerable<TreeViewNodeModelBase> selectedNodes)
        {
            _selectedNodes = selectedNodes;

            UpdateIndicators();
        }
        private void UpdateIndicators()
        {
            _selectedCount = 0;
            _selectedFileCount = 0;
            _selectedDirectoryCount = 0;
            _totalDirectoryCount = 0;
            _totalFileCount = 0;

            this.ForEach<TreeViewNodeModelBase>(node =>
            {
                if (node is not FileTreeNodeViewModel)
                    return;

                var fileNode = node as FileTreeNodeViewModel;

                if (fileNode.IsDirectory)
                {
                    _totalDirectoryCount++;

                    if (fileNode.IsSelected)
                        _selectedDirectoryCount++;
                }
                else
                {
                    _totalFileCount++;

                    if (fileNode.IsSelected)
                        _selectedFileCount++;
                }

                if (fileNode.IsSelected)
                    _selectedCount++;
            });
        }
    }
}