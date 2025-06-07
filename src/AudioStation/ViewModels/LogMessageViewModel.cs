using AudioStation.Model;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class LogMessageViewModel : ViewModelBase
    {
        string _message;
        LogMessageSeverity _severity;
        LogMessageType _type;

        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }
        public LogMessageSeverity Severity
        {
            get { return _severity; }
            set { this.RaiseAndSetIfChanged(ref _severity, value); }
        }
        public LogMessageType Type
        {
            get { return _type; }
            set { this.RaiseAndSetIfChanged(ref _type, value); }
        }

        public LogMessageViewModel()
        {
            this.Message = string.Empty;
        }
    }
}
