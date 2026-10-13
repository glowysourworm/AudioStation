using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels.LibraryTree
{
    public abstract class LibraryTreeNodeBase : TreeViewNodeModelBase
    {
        bool _duplicate;
        string _duplicateMessage;

        public bool Duplicate
        {
            get { return _duplicate; }
            protected set { this.RaiseAndSetIfChanged(ref _duplicate, value); }
        }
        public string DuplicateMessage
        {
            get { return _duplicateMessage; }
            protected set { this.RaiseAndSetIfChanged(ref _duplicateMessage, value); }
        }

        protected LibraryTreeNodeBase(int recursionDepth, TreeViewNodeModelBase? parent) : base(recursionDepth, parent)
        {
            _duplicate = false;
            _duplicateMessage = string.Empty;
        }

        public void SetDuplicate(string message)
        {
            this.Duplicate = true;
            this.DuplicateMessage = message;
        }
        public void ClearDuplicate()
        {
            this.DuplicateMessage = string.Empty;
            this.Duplicate = false;
        }
    }
}
