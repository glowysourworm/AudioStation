using SimpleWpf.Extensions;

namespace AudioStation.Event.EventViewModel
{
    public class DialogSplashScreenViewModel : DialogViewModelBase
    {
        double _progress;
        string _message;

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

        public DialogSplashScreenViewModel()
        {
            this.Message = string.Empty;
        }
    }
}
