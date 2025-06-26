using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.PlaylistViewModels.Interface;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.PlaylistViewModels
{
    public class PlaylistEntryViewModel : ViewModelBase, IPlaylistEntryViewModel
    {
        ArtistViewModel _artist;
        AlbumViewModel _album;
        LibraryEntryViewModel _track;

        TimeSpan _currentTime;
        double _currentTimeRatio;
        bool _isPlaying;

        public ArtistViewModel Artist
        {
            get { return _artist; }
            set { RaiseAndSetIfChanged(ref _artist, value); }
        }
        public AlbumViewModel Album
        {
            get { return _album; }
            set { RaiseAndSetIfChanged(ref _album, value); }
        }
        public LibraryEntryViewModel Track
        {
            get { return _track; }
            set { RaiseAndSetIfChanged(ref _track, value); }
        }
        public TimeSpan CurrentTime
        {
            get { return _currentTime; }
            set { RaiseAndSetIfChanged(ref _currentTime, value); UpdateCurrentTime(_currentTime); }
        }
        public double CurrentTimeRatio
        {
            get { return _currentTimeRatio; }
            set { RaiseAndSetIfChanged(ref _currentTimeRatio, value); }
        }
        public bool IsPlaying
        {
            get { return _isPlaying; }
            set { this.RaiseAndSetIfChanged(ref _isPlaying, value); }
        }
        public void UpdateCurrentTime(TimeSpan currentTime)
        {
            _currentTime = currentTime;

            if (this.Track.Duration.TotalMilliseconds <= 0)
                this.CurrentTimeRatio = 0;
            else
                this.CurrentTimeRatio = this.CurrentTime.TotalMilliseconds / this.Track.Duration.TotalMilliseconds;

            // In case this event didn't get raised (there's two code paths here..)
            OnPropertyChanged("CurrentTime");
        }

        public PlaylistEntryViewModel(ArtistViewModel artist, AlbumViewModel album, LibraryEntryViewModel track)
        {
            this.Artist = artist;
            this.Album = album;
            this.Track = track;
            this.CurrentTime = TimeSpan.Zero;
            this.CurrentTimeRatio = 0;
            this.IsPlaying = false;
        }
    }
}
