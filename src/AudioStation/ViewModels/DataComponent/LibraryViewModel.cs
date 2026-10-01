using System.Collections.ObjectModel;

using AudioStation.Core.Model.Interface;
using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.ViewModels.DataComponent
{
    public class LibraryViewModel : DataComponentViewModelBase
    {
        ObservableCollection<TrackViewModel> _tracks;
        ObservableCollection<AlbumViewModel> _albums;
        ObservableCollection<ArtistViewModel> _artists;
        ObservableCollection<GenreViewModel> _genres;

        public ObservableCollection<TrackViewModel> Tracks
        {
            get { return _tracks; }
            set { RaiseAndSetIfChanged(ref _tracks, value); }
        }
        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set { RaiseAndSetIfChanged(ref _albums, value); }
        }
        public ObservableCollection<ArtistViewModel> Artists
        {
            get { return _artists; }
            set { RaiseAndSetIfChanged(ref _artists, value); }
        }
        public ObservableCollection<GenreViewModel> Genres
        {
            get { return _genres; }
            set { RaiseAndSetIfChanged(ref _genres, value); }
        }

        /// <summary>
        /// This instance should be owned by the LibraryManagerViewModel. The primary view model (main) 
        /// will have the manager view model injected (as a pattern).
        /// </summary>
        public LibraryViewModel() : base("Library")
        {
            this.Tracks = new ObservableCollection<TrackViewModel>();
            this.Albums = new ObservableCollection<AlbumViewModel>();
            this.Artists = new ObservableCollection<ArtistViewModel>();
            this.Genres = new ObservableCollection<GenreViewModel>();
        }

        public override void Initialize(IAudioStationConfiguration configuration)
        {
            // Nothing to do
        }

        public override void Dispose()
        {
            this.Tracks.Clear();
            this.Albums.Clear();
            this.Artists.Clear();
            this.Genres.Clear();
        }
    }
}
