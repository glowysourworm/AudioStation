using System.Collections.ObjectModel;

using AudioStation.Model;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LogViewModels
{
    public class LogSetViewModel : ViewModelBase
    {
        ObservableCollection<LogComponentViewModel> _logs;

        public ObservableCollection<LogComponentViewModel> Logs
        {
            get { return _logs; }
            set { this.RaiseAndSetIfChanged(ref _logs, value); }
        }

        public LogSetViewModel()
        {
            this.Logs = new ObservableCollection<LogComponentViewModel>();
        }

        public LogComponentViewModel GetLog(LogMessage message)
        {
            return this.Logs.FirstOrDefault(x => x.Name == message.GetLogName());
        }
    }
}
