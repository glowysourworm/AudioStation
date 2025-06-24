using System.Collections.ObjectModel;

using AudioStation.ViewModels.PlaylistViewModels.Interface;
using AudioStation.ViewModels.Vendor.AudioDBViewModel;
using AudioStation.ViewModels.Vendor.LastFmViewModel;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        IPlaylistEntryViewModel _nowPlaying;
        ObservableCollection<IPlaylistEntryViewModel> _entries;

        // Vendor Information
        LastFmNowPlayingViewModel _lastFmNowPlaying;
        AudioDBNowPlayingViewModel _audioDBNowPlaying;

        public ObservableCollection<IPlaylistEntryViewModel> Entries
        {
            get { return _entries; }
            set { this.RaiseAndSetIfChanged(ref _entries, value); }
        }
        public IPlaylistEntryViewModel NowPlaying
        {
            get { return _nowPlaying; }
            set { this.RaiseAndSetIfChanged(ref _nowPlaying, value); }
        }
        public LastFmNowPlayingViewModel LastFmNowPlaying
        {
            get { return _lastFmNowPlaying; }
            set { this.RaiseAndSetIfChanged(ref _lastFmNowPlaying, value); }
        }
        public AudioDBNowPlayingViewModel AudioDBNowPlaying
        {
            get { return _audioDBNowPlaying; }
            set { this.RaiseAndSetIfChanged(ref _audioDBNowPlaying, value); }
        }

        public PlaylistViewModel()
        {
            this.Entries = new ObservableCollection<IPlaylistEntryViewModel>();
            this.NowPlaying = null;
            this.LastFmNowPlaying = null;
            this.AudioDBNowPlaying = null;
        }
    }
}
