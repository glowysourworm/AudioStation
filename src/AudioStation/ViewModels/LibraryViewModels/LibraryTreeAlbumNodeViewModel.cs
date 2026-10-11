using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class LibraryTreeAlbumNodeViewModel : TreeViewNodeModelBase
    {
        private AlbumViewModel _album;

        public override object Key
        {
            get { return _album.Album; }
        }


        public LibraryTreeAlbumNodeViewModel(int recursionDepth, AlbumViewModel album, TreeViewNodeModelBase? parent)
            : base(recursionDepth, parent)
        {
            if (album == null || string.IsNullOrWhiteSpace(album.Album))
                throw new ArgumentException("Invalid Album Node");

            _album = album;
        }
    }
}
