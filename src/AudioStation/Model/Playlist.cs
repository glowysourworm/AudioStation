using AudioStation.ViewModel.LibraryViewModel;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryViewModel.Comparer;

using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.Model
{
    public class Playlist : ViewModelBase
    {
        string _name;
        int _nowPlayingIndex;
        SortedObservableCollection<TitleViewModel> _tracks;

        public string Name
        {
            get { return _name; }
            set { this.SetProperty(ref _name, value); }
        }
        public int NowPlayingIndex
        {
            get { return _nowPlayingIndex; }
            set { this.SetProperty(ref _nowPlayingIndex, value); }
        }
        public SortedObservableCollection<TitleViewModel> Tracks
        {
            get { return _tracks; }
            set { this.SetProperty(ref _tracks, value); }
        }
        public Playlist()
        {
            this.Name = string.Empty;
            this.NowPlayingIndex = -1;
            this.Tracks = new SortedObservableCollection<TitleViewModel>(new TitleViewModelDefaultComparer());
        }
    }
}
