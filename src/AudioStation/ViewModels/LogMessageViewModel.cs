using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels
{
    public class LogMessageViewModel : ViewModelBase
    {
        string _message;
        LogLevel _level;
        LogMessageType _type;

        public string Message
        {
            get { return _message; }
            set { this.RaiseAndSetIfChanged(ref _message, value); }
        }
        public LogLevel Level
        {
            get { return _level; }
            set { this.RaiseAndSetIfChanged(ref _level, value); }
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
