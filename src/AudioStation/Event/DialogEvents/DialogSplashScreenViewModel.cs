using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.Event.DialogEvents
{
    public class DialogSplashScreenViewModel : ViewModelBase
    {
        double _progress;
        string _message;
        bool _showProgressBar;
        bool _showProgressMessage;

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
        public bool ShowProgressBar
        {
            get { return _showProgressBar; }
            set { this.RaiseAndSetIfChanged(ref _showProgressBar, value); }
        }
        public bool ShowProgressMessage
        {
            get { return _showProgressMessage; }
            set { this.RaiseAndSetIfChanged(ref _showProgressMessage, value); }
        }

        public DialogSplashScreenViewModel()
        {
            this.Message = string.Empty;
        }
    }
}
