using System.Collections.ObjectModel;

using AudioStation.ViewModels.MainViewModels.Interface;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.MainViewModels
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
