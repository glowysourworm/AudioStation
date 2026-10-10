using AudioStation.Core.Model.Interface;
using AudioStation.ViewModels.LibraryViewModels;

using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.ViewModels.DataComponent
{
    public class LibraryViewModel : DataComponentViewModelBase
    {
        KeyedObservableCollection<int, TrackViewModel> _tracks;
        KeyedObservableCollection<int, AlbumViewModel> _albums;
        KeyedObservableCollection<int, ArtistViewModel> _artists;
        KeyedObservableCollection<int, GenreViewModel> _genres;
        KeyedObservableCollection<int, PlaylistViewModel> _playlists;

        public IReadOnlyCollection<TrackViewModel> Tracks
        {
            get { return _tracks; }
        }
        public IReadOnlyCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
        }
        public IReadOnlyCollection<ArtistViewModel> Artists
        {
            get { return _artists; }
        }
        public IReadOnlyCollection<GenreViewModel> Genres
        {
            get { return _genres; }
        }
        public IReadOnlyCollection<PlaylistViewModel> Playlists
        {
            get { return _playlists; }
        }

        /// <summary>
        /// This instance should be owned by the LibraryManagerViewModel. The primary view model (main) 
        /// will have the manager view model injected (as a pattern).
        /// </summary>
        public LibraryViewModel() : base("Library")
        {
            _tracks = new KeyedObservableCollection<int, TrackViewModel>();
            _albums = new KeyedObservableCollection<int, AlbumViewModel>();
            _artists = new KeyedObservableCollection<int, ArtistViewModel>();
            _genres = new KeyedObservableCollection<int, GenreViewModel>();
            _playlists = new KeyedObservableCollection<int, PlaylistViewModel>();
        }

        public GenreViewModel GetGenre(int genreId)
        {
            return _genres[genreId];
        }
        public ArtistViewModel GetArtist(int artistId)
        {
            return _artists[artistId];
        }
        public AlbumViewModel GetAlbum(int albumId)
        {
            return _albums[albumId];
        }
        public TrackViewModel GetTrack(int trackId)
        {
            return _tracks[trackId];
        }
        public PlaylistViewModel GetPlaylist(int playlistId)
        {
            return _playlists[playlistId];
        }

        public bool ContainsGenre(int id)
        {
            return _genres.ContainsKey(id);
        }
        public bool ContainsArtist(int id)
        {
            return _artists.ContainsKey(id);
        }
        public bool ContainsAlbum(int id)
        {
            return _albums.ContainsKey(id);
        }
        public bool ContainsTrack(int id)
        {
            return _tracks.ContainsKey(id);
        }

        public void AddGenre(GenreViewModel genre)
        {
            if (_genres.ContainsKey(genre.Id))
                throw new ArgumentException("Genre already contained in the library");

            _genres.Add(genre.Id, genre);
        }
        public void AddArtist(ArtistViewModel artist)
        {
            if (_artists.ContainsKey(artist.Id))
                throw new ArgumentException("Artist already contained in the library");

            _artists.Add(artist.Id, artist);
        }
        public void AddAlbum(AlbumViewModel album)
        {
            if (_albums.ContainsKey(album.Id))
                throw new ArgumentException("Album already contained in the library");

            _albums.Add(album.Id, album);
        }
        public void AddTrack(TrackViewModel track)
        {
            if (_tracks.ContainsKey(track.Id))
                throw new ArgumentException("Track already contained in the library");

            _tracks.Add(track.Id, track);
        }
        public void AddGenre(PlaylistViewModel playlist)
        {
            if (_playlists.ContainsKey(playlist.Id))
                throw new ArgumentException("Playlist already contained in the library");

            _playlists.Add(playlist.Id, playlist);
        }

        public override void Initialize(IAudioStationConfiguration configuration)
        {
            // Nothing to do
        }

        public override void Dispose()
        {
            _tracks.Clear();
            _albums.Clear();
            _artists.Clear();
            _genres.Clear();
            _playlists.Clear();
        }
    }
}
