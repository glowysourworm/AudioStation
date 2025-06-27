using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.LogViewModels
{
    public class LogMessageViewModel : ViewModelBase
    {
        string _message;
        LogLevel _level;
        LogMessageType _type;
        DateTime _timestamp;

        public string Message
        {
            get { return _message; }
            set { RaiseAndSetIfChanged(ref _message, value); }
        }
        public LogLevel Level
        {
            get { return _level; }
            set { RaiseAndSetIfChanged(ref _level, value); }
        }
        public LogMessageType Type
        {
            get { return _type; }
            set { RaiseAndSetIfChanged(ref _type, value); }
        }
        public DateTime Timestamp
        {
            get { return _timestamp; }
            set { this.RaiseAndSetIfChanged(ref _timestamp, value); }
        }

        public LogMessageViewModel()
        {
            this.Message = string.Empty;
        }
    }
}
