using System.Collections.ObjectModel;

using AudioStation.ViewModels.PlaylistViewModels.Interface;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        IPlaylistEntryViewModel _nowPlaying;
        ObservableCollection<IPlaylistEntryViewModel> _entries;

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

        public PlaylistViewModel()
        {
            this.Entries = new ObservableCollection<IPlaylistEntryViewModel>();
            this.NowPlaying = null;
        }
    }
}
