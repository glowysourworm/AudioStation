using AudioStation.Core.Model;

using SimpleWpf.UI.ViewModel.FileTreeView;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels.LibraryTree
{
    /// <summary>
    /// Node for using showing a file / folder tree that has entity data formatted into the path names. 
    /// </summary>
    public class FileTreeFormatNodeViewModel : FileTreeNodeViewModel
    {
        // File / Folder Formatting:  The directory structure will have a format that may contain
        //                            multiple field names for the track grouping.
        //

        string _duplicateMessage;
        string _validationMessage;
        bool _isValid;
        bool _isDuplicate;

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

        private LibraryGrouping _grouping;


        public FileTreeFormatNodeViewModel(int recursionDepth,
                                           string path,
                                           int directoryFileCount,
                                           TrackGroupingType directoryFormat,
                                           string? customFormat,
                                           TreeViewNodeModelBase? parent)

            : base(recursionDepth, path, directoryFileCount, parent)
        {
            // Create a grouping object for handling the format parsing
            //
            _grouping = new LibraryGrouping(directoryFormat, customFormat);

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
