using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels.LibraryTree
{
    public class LibraryTreeMediaFormatNodeViewModel : TreeViewNodeModelBase
    {
        private string _mediaFormat;

        public override object Key
        {
            get { return _mediaFormat; }
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
