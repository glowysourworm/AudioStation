using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels.LibraryTree
{
    public class LibraryTreeAlbumMediaNodeViewModel : TreeViewNodeModelBase
    {
        private object _key;

        public override object Key
        {
            get { return _key; }
        }

        public LibraryTreeAlbumMediaNodeViewModel(int recursionDepth, string albumName, int mediaNumber, int mediaTotal, TreeViewNodeModelBase? parent)
            : base(recursionDepth, parent)
        {
            if (!ValidateKey(mediaNumber, mediaTotal, albumName))
                throw new ArgumentException("Invalid Media Node");

            _key = GenerateKey(mediaNumber, mediaTotal, albumName);
        }

        public static bool ValidateKey(int mediaNumber, int mediaTotal, string albumName)
        {
            //return mediaNumber > 0 &&
            //       mediaTotal > 0 &&
            //       !string.IsNullOrWhiteSpace(albumName);

            return true;
        }
        public static object GenerateKey(int mediaNumber, int mediaTotal, string albumName)
        {
            if (!ValidateKey(mediaNumber, mediaTotal, albumName))
                throw new ArgumentException("Invalid album media node key generation. Please first validate fields.");

            // We're going to handle key generation here; and use defaults for grouping tracks 
            // until the media numbers are setup.

            var albumName_ = albumName;
            var mediaTotal_ = mediaTotal;
            var mediaNumber_ = mediaNumber;
            var improper = false;

            if (string.IsNullOrWhiteSpace(albumName_))
            {
                albumName_ = "(Unknown)";
                improper = true;
            }


            if (mediaTotal_ <= 0)
                improper = true;

            if (mediaNumber_ <= 0)
                improper = true;

            if (mediaTotal_ < mediaNumber_)
                improper = true;

            if (improper)
                return string.Format("Album {0} Media Invalid", albumName_);

            else
                return string.Format("{0} Media {1} of {2}", albumName_, mediaNumber_, mediaTotal_);
        }
    }
}
