using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels.LibraryTree
{
    public class LibraryTreeMediaFormatNodeViewModel : LibraryTreeNodeBase
    {
        private string _mediaFormat;

        public override object Key
        {
            get { return _mediaFormat; }
        }
        public string MediaFormat
        {
            get { return _mediaFormat; }
            set { this.RaiseAndSetIfChanged(ref _mediaFormat, value); }
        }

        public LibraryTreeMediaFormatNodeViewModel(int recursionDepth, string mediaFormat, TreeViewNodeModelBase? parent)
            : base(recursionDepth, parent)
        {
            if (string.IsNullOrWhiteSpace(mediaFormat))
                throw new ArgumentException("Invalid Media Format Node");

            _mediaFormat = mediaFormat;
        }
    }
}
