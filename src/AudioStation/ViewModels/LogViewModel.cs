using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Utility;
using AudioStation.EventHandler;
using AudioStation.Model;
using AudioStation.ViewModels.LogViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

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

        public override void Initialize(DialogProgressHandler progressHandler)
        {
            
        }

        public override void Dispose()
        {
        }

        private void OnLog(LogMessage message)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(OnLog, DispatcherPriority.Background, message);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (!_logs.Any(log => log.Id.Equals(message.LogComponentId)))
                {
                    _logs.Add(new LogComponentViewModel()
                    {
                        Id = message.LogComponentId,
                        LogLevel = LogLevel.None,               // This is a user input (for filtering)
                        Name = message.IsSeparatedLog ? "Separate Log #" + message.LogComponentId.ToString() : message.Type.ToString()
                    });
                }

                var log = _logs.First(x => x.Id.Equals(message.LogComponentId));

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
