using AudioStation.Core.Model;

using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.ViewModels.LibraryViewModels.LibraryTree
{
    /// <summary>
    /// Combined entity node for anything required in the library to show in the simple tree view. The reason
    /// for the combined entity is the custom formats allowed in the file and folder names. So, there can be
    /// multiple entity types per node.
    /// </summary>
    public class LibraryTreeEntityNodeViewModel : LibraryTreeNodeBase
    {
        private ArtistViewModel? _artist;
        private AlbumViewModel? _album;
        private GenreViewModel? _genre;
        private string? _mediaFormat;
        private int? _mediaNumber;
        private int? _mediaTotal;

        object? _key;

        public override object Key
        {
            get
            {
                if (_key == null)
                    throw new Exception("Key has not been set for node. This happened because no entities have been added to the node.");

                return _key;
            }
        }

        public bool IsSet()
        {
            return _key != null;
        }

        public LibraryTreeEntityNodeViewModel(int recursionDepth, TreeViewNodeModelBase? parent)
            : this(recursionDepth, null, null, null, null, null, null, parent)
        {
            _key = null;
        }
        public LibraryTreeEntityNodeViewModel(int recursionDepth, ArtistViewModel artist, TreeViewNodeModelBase? parent)
            : this(recursionDepth, artist, null, null, null, null, null, parent)
        {
            _key = GenerateKey(artist);
        }
        public LibraryTreeEntityNodeViewModel(int recursionDepth, AlbumViewModel album, TreeViewNodeModelBase? parent)
            : this(recursionDepth, null, album, null, null, null, null, parent)
        {
            _key = GenerateKey(album);
        }
        public LibraryTreeEntityNodeViewModel(int recursionDepth, GenreViewModel genre, TreeViewNodeModelBase? parent)
            : this(recursionDepth, null, null, genre, null, null, null, parent)
        {
            _key = GenerateKey(genre);
        }
        public LibraryTreeEntityNodeViewModel(int recursionDepth, int mediaNumber, int mediaTotal, string mediaFormat, TreeViewNodeModelBase? parent)
            : this(recursionDepth, null, null, null, mediaNumber, mediaTotal, mediaFormat, parent)
        {
            _key = GenerateKey(mediaNumber, mediaTotal, mediaFormat);
        }
        protected LibraryTreeEntityNodeViewModel(int recursionDepth,
                                                 ArtistViewModel? artist,
                                                 AlbumViewModel? album,
                                                 GenreViewModel? genre,
                                                 int? mediaNumber,
                                                 int? mediaTotal,
                                                 string? mediaFormat,
                                                 TreeViewNodeModelBase? parent)
            : base(recursionDepth, parent)
        {
            _artist = artist;
            _album = album;
            _genre = genre;
            _mediaNumber = mediaNumber;
            _mediaTotal = mediaTotal;
            _mediaFormat = mediaFormat;
        }

        public bool ContainsEntityType(LibraryEntryType type)
        {
            switch (type)
            {
                case LibraryEntryType.Album:
                    return _album != null;
                case LibraryEntryType.Artist:
                    return _artist != null;
                case LibraryEntryType.Genre:
                    return _genre != null;
                case LibraryEntryType.MediaFormat:
                    return _mediaFormat != null && _mediaNumber != null && _mediaTotal != null;
                case LibraryEntryType.Track:
                case LibraryEntryType.Playlist:
                case LibraryEntryType.PlaylistEntry:
                default:
                    throw new Exception("Unhandled Library Entity Type");
            }
        }

        public void AddEntity(ArtistViewModel artist)
        {
            if (ContainsEntityType(LibraryEntryType.Artist))
                throw new Exception("Artist entity already set for this node");

            _artist = artist;
        }
        public void AddEntity(AlbumViewModel album)
        {
            if (ContainsEntityType(LibraryEntryType.Album))
                throw new Exception("Album entity already set for this node");

            _album = album;
        }
        public void AddEntity(GenreViewModel genre)
        {
            if (ContainsEntityType(LibraryEntryType.Genre))
                throw new Exception("Genre entity already set for this node");

            _genre = genre;
        }
        public void AddEntity(int mediaNumber, int mediaTotal, string mediaFormat)
        {
            if (ContainsEntityType(LibraryEntryType.MediaFormat))
                throw new Exception("Media format entity already set for this node");

            _mediaFormat = mediaFormat;
            _mediaNumber = mediaNumber;
            _mediaTotal = mediaTotal;
        }

        public static bool ValidateKey(int mediaNumber, int mediaTotal, string mediaFormat)
        {
            //return mediaNumber > 0 &&
            //       mediaTotal > 0 &&
            //       !string.IsNullOrWhiteSpace(albumName);

            return true;
        }
        public static bool ValidateKey(ArtistViewModel artist)
        {
            return artist.Id != Guid.Empty || !string.IsNullOrWhiteSpace(artist.Artist);
        }
        public static bool ValidateKey(AlbumViewModel album)
        {
            return album.Id != Guid.Empty || !string.IsNullOrWhiteSpace(album.Album);
        }
        public static bool ValidateKey(GenreViewModel genre)
        {
            return genre.Id != Guid.Empty || !string.IsNullOrWhiteSpace(genre.Name);
        }
        public static object GenerateKey(AlbumViewModel album)
        {
            return album.Id == Guid.Empty ? album.Album : album.Id.ToString();
        }
        public static object GenerateKey(ArtistViewModel artist)
        {
            return artist.Id == Guid.Empty ? artist.Artist : artist.Id.ToString();
        }
        public static object GenerateKey(GenreViewModel genre)
        {
            return genre.Id == Guid.Empty ? genre.Name : genre.Id.ToString();
        }
        public static object GenerateKey(int mediaNumber, int mediaTotal, string mediaFormat)
        {
            if (!ValidateKey(mediaNumber, mediaTotal, mediaFormat))
                throw new ArgumentException("Invalid album media node key generation. Please first validate fields.");

            // We're going to handle key generation here; and use defaults for grouping tracks 
            // until the media numbers are setup.

            var mediaFormat_ = mediaFormat;
            var mediaTotal_ = mediaTotal;
            var mediaNumber_ = mediaNumber;
            var improper = false;

            if (string.IsNullOrWhiteSpace(mediaFormat_))
            {
                mediaFormat_ = "Format Unknown";
                improper = true;
            }


            if (mediaTotal_ <= 0)
                improper = true;

            if (mediaNumber_ <= 0)
                improper = true;

            if (mediaTotal_ < mediaNumber_)
                improper = true;

            if (improper)
                return string.Format("Media Invalid");

            else
                return string.Format("{0} {1} of {2}", mediaFormat_, mediaNumber_, mediaTotal_);
        }
    }
}
