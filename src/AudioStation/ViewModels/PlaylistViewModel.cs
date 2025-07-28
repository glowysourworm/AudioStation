using System.Collections.ObjectModel;

using AudioStation.ViewModels.PlaylistViewModels.Interface;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        IPlaylistEntryViewModel _currentTrack;
        ObservableCollection<IPlaylistEntryViewModel> _entries;

        public ObservableCollection<IPlaylistEntryViewModel> Entries
        {
            get { return _entries; }
            set { this.RaiseAndSetIfChanged(ref _entries, value); }
        }
        public IPlaylistEntryViewModel CurrentTrack
        {
            get { return _currentTrack; }
            set { this.RaiseAndSetIfChanged(ref _currentTrack, value); }
        }

        public PlaylistViewModel()
        {
            this.Entries = new ObservableCollection<IPlaylistEntryViewModel>();
            this.CurrentTrack = null;
        }
    }
}
