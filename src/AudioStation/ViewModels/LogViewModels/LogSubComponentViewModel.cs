using System.Collections.ObjectModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.LogViewModels
{
    public class LogSubComponentViewModel : ViewModelBase
    {
        int _id;
        string _name;
        LogLevel _logLevel;
        ObservableCollection<LogMessageViewModel> _messages;

        public int Id
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

        public LogSubComponentViewModel()
        {
            this.Name = string.Empty;
            this.Messages = new ObservableCollection<LogMessageViewModel>();
            this.LogLevel = LogLevel.None;
            this.Id = -1;
        }
    }
}
