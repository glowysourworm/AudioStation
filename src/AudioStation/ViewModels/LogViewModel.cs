using System.Collections.ObjectModel;
using System.Windows.Threading;

using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.LogViewModels;

using Microsoft.Extensions.Logging;

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

            // Go ahead and add logs for the base message types
            foreach (LogMessageType type in Enum.GetValues(typeof(LogMessageType)))
            {
                this.Logs.Add(new LogComponentViewModel()
                {
                    Id = type,
                    LogLevel = LogLevel.None,               // This is a user input (for filtering)
                    Name = type.ToString()
                });
            }
        }

        public override Task Initialize(DialogProgressHandler progressHandler)
        {
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
        }

        private void OnLog(LogMessage message)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                ApplicationHelpers.BeginInvokeDispatcher(OnLog, DispatcherPriority.Background, message);

            else
            {
                if (!_logs.Any(log => log.Id.Equals(message.LogId)))
                {
                    _logs.Add(new LogComponentViewModel()
                    {
                        Id = message.LogId,
                        LogLevel = LogLevel.None,               // This is a user input (for filtering)
                        Name = message.Type.ToString()
                    });
                }

                var log = _logs.First(x => x.Id.Equals(message.LogId));

                log.Messages.Insert(0, new LogMessageViewModel()
                {
                    Level = message.Level,
                    Message = message.Message,
                    Type = message.Type,
                    Timestamp = message.Timestamp
                });
            }
        }
    }
}
