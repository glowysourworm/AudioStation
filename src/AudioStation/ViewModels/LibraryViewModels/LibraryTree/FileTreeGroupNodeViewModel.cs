using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels.LibraryTree
{
    /// <summary>
    /// Node for showing grouping issues prior to import; or when browsing the library files.
    /// </summary>
    public class FileTreeGroupNodeViewModel : FileTreeNodeViewModel
    {
        // Grouping:  Issues tend to be based on non-matching, duplicate, or string format 
        //            related data. These will be shown to the user by adding a couple of
        //            extra fields.

        string _groupFieldName;
        string _duplicateMessage;
        string _validationMessage;
        bool _isValid;
        bool _isDuplicate;

        public string GroupFieldName
        {
            get { return _groupFieldName; }
            set { this.RaiseAndSetIfChanged(ref _groupFieldName, value); }
        }
        public string DuplicateMessage
        {
            get { return _duplicateMessage; }
            protected set { this.RaiseAndSetIfChanged(ref _duplicateMessage, value); }
        }
        public string ValidationMessage
        {
            get { return _validationMessage; }
            protected set { this.RaiseAndSetIfChanged(ref _validationMessage, value); }
        }
        public bool IsValid
        {
            get { return _isValid; }
            protected set { this.RaiseAndSetIfChanged(ref _isValid, value); }
        }
        public bool IsDuplicate
        {
            get { return _isDuplicate; }
            protected set { this.RaiseAndSetIfChanged(ref _isDuplicate, value); }
        }


        public FileTreeGroupNodeViewModel(int recursionDepth, string groupFieldName, string path, int directoryFileCount, TreeViewNodeModelBase? parent)
            : base(recursionDepth, path, directoryFileCount, parent)
        {
            _groupFieldName = groupFieldName;
            _validationMessage = string.Empty;
            _duplicateMessage = string.Empty;
        }
        public void SetInvalid(string messasge)
        {
            this.IsValid = false;
            this.ValidationMessage = messasge;
        }
        public void SetDuplicate(string message)
        {
            this.IsDuplicate = true;
            this.DuplicateMessage = message;
        }
        public void ClearDuplicate()
        {
            this.DuplicateMessage = string.Empty;
            this.IsDuplicate = false;
        }
        public void ClearValidation()
        {
            this.IsValid = true;
            this.ValidationMessage = string.Empty;
        }
    }
}
