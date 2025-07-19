namespace AudioStation.Event.DialogEvents
{
    public class DialogLoadingViewModel : DialogViewModelBase
    {
        bool _showProgressBar;
        double _progress;
        string _message;
        string _title;

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
        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }

        public DialogLoadingViewModel()
        {
            this.Message = string.Empty;
            this.Title = string.Empty;
        }
    }
}
