using System.Collections.ObjectModel;
using System.Windows.Threading;

using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.LogViewModels;

using SimpleWpf.IocFramework.EventAggregation;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels
{
    public class LogViewModel : PrimaryViewModelBase
    {
        private readonly IOutputController _outputController;

        ObservableCollection<LogComponentViewModel> _logs;

        public ObservableCollection<LogComponentViewModel> Logs
        {
            get { return _logs; }
            set { this.RaiseAndSetIfChanged(ref _logs, value); }
        }

        public LogViewModel(IIocEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);

            this.Logs = new ObservableCollection<LogComponentViewModel>();
        }

        public override Task Initialize(DialogProgressHandler progressHandler)
        {
            return Task.CompletedTask;
        }

        private void OnLog(LogMessage message)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                ApplicationHelpers.BeginInvokeDispatcher(OnLog, DispatcherPriority.Background, message);

            else
            {
                var log = _logs.FirstOrDefault(x => x.Name == message.GetLogName());
                {

                }

                // New Log
                if (log == null)
                {
                    log = new LogComponentViewModel()
                    {
                        Name = message.GetLogName()
                    };

                    this.Logs.Add(log);
                }

                // Check Sub Log(s)
                var subLog = log.SubComponents.FirstOrDefault(x => x.Name == message.GetSubLogName());

                // New Sublog
                if (subLog == null)
                {
                    subLog = new LogSubComponentViewModel()
                    {
                        Name = message.GetSubLogName()
                    };

                    log.SubComponents.Add(subLog);
                }

                subLog.Messages.Insert(0, new LogMessageViewModel()
                {
                    Level = message.Level,
                    Message = message.Message,
                    Type = message.Type,
                    Timestamp = message.Timestamp
                });
            }
        }

        public override void Dispose()
        {
        }
    }
}
