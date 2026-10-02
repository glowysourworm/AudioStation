using System.Collections.ObjectModel;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class PlaylistViewModel : ViewModelBase
    {
        int _id;
        string _name;

        // Each entry will have all the data needed to reconstruct the album-based grouping. There
        // will need to be a better way to call tracks from the library, in playlist order, when
        // the playlist is loaded.
        ObservableCollection<PlaylistEntryViewModel> _entries;

        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public ObservableCollection<PlaylistEntryViewModel> Entries
        {
            get { return _entries; }
            set { this.RaiseAndSetIfChanged(ref _entries, value); }
        }

        public PlaylistViewModel()
        {
            this.Id = 0;
            this.Name = "New Playlist";
            this.Entries = new ObservableCollection<PlaylistEntryViewModel>();
        }
    }
}
