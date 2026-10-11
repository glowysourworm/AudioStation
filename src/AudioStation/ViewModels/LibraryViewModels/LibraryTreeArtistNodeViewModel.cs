using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class LibraryTreeArtistNodeViewModel : TreeViewNodeModelBase
    {
        private ArtistViewModel _artist;

        public override object Key
        {
            get { return _artist.Artist; }
        }


        public LibraryTreeArtistNodeViewModel(int recursionDepth, ArtistViewModel artist, TreeViewNodeModelBase? parent)
            : base(recursionDepth, parent)
        {
            if (artist == null || string.IsNullOrWhiteSpace(artist.Artist))
                throw new ArgumentException("Invalid Artist Node");

            _artist = artist;
        }
    }
}
