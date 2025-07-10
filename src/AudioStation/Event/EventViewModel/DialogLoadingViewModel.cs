using SimpleWpf.Extensions;

namespace AudioStation.Event.EventViewModel
{
    public class DialogLoadingViewModel : DialogViewModelBase
    {
        bool _showProgressBar;
        double _progress;
        string _message;

        public bool ShowProgressBar
        {
            get { return _showProgressBar; }
            set { this.RaiseAndSetIfChanged(ref _showProgressBar, value); }
        }
        public double Progress
        {
            get { return _progress; }
            set { this.RaiseAndSetIfChanged(ref _progress, value); }
        }
        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }

        public DialogLoadingViewModel()
        {
            this.Message = string.Empty;
        }
    }
}
