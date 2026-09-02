using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels
{
    public class CDImporterTrackViewModel : ViewModelBase
    {
        int _track;
        double _progress;
        bool _complete;

        public int Track
        {
            get { return _track; }
            set { this.RaiseAndSetIfChanged(ref _track, value); }
        }
        public double Progress
        {
            get { return _progress; }
            set { this.RaiseAndSetIfChanged(ref _progress, value); }
        }
        public bool Complete
        {
            get { return _complete; }
            set { this.RaiseAndSetIfChanged(ref _complete, value); }
        }
    }
}
