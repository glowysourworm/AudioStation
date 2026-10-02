using AudioStation.Utility;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class MediaViewModel : ViewModelBase
    {
        int _mediaNumber;
        TimeSpan _duration;
        SortedObservableCollection<TrackViewModel> _tracks;

        public int MediaNumber
        {
            get { return _mediaNumber; }
            set { this.RaiseAndSetIfChanged(ref _mediaNumber, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }
        public SortedObservableCollection<TrackViewModel> Tracks
        {
            get { return _tracks; }
            set { this.RaiseAndSetIfChanged(ref _tracks, value); }
        }

        public MediaViewModel()
        {
            this.Tracks = new SortedObservableCollection<TrackViewModel>(new PropertyComparer<int, TrackViewModel>(x => x.TrackNumber));
            this.Duration = TimeSpan.Zero;
        }
    }
}
