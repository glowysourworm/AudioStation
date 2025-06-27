using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.LogViewModels
{
    public class LogComponentViewModel : ViewModelBase
    {
        object _id;
        string _name;
        LogLevel _logLevel;
        ObservableCollection<LogMessageViewModel> _messages;

        public object Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public LogLevel LogLevel
        {
            get { return _logLevel; }
            set { this.RaiseAndSetIfChanged(ref _logLevel, value); }
        }
        public ObservableCollection<LogMessageViewModel> Messages
        {
            get { return _messages; }
            set { this.RaiseAndSetIfChanged(ref _messages, value); }
        }

        public LogComponentViewModel()
        {
            this.Name = string.Empty;
            this.Messages = new ObservableCollection<LogMessageViewModel>();
            this.LogLevel = LogLevel.None;
            this.Id = null;
        }
    }
}
