using System.Collections.ObjectModel;
using System.Windows.Threading;

using AudioStation.Core.Event;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.ViewModels.DataComponent.LogViewModels;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

namespace AudioStation.ViewModels.DataComponent
{
    public class LogViewModel : DataComponentViewModelBase
    {
        LogSetViewModel _viewModel;

        public ObservableCollection<LogComponentViewModel> Logs
        {
            get { return _viewModel.Logs; }
        }

        public LogViewModel(IIocEventAggregator eventAggregator) : base("Log")
        {
            _viewModel = new LogSetViewModel();

            eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);
        }

        private void OnLog(LogMessage message)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.BeginInvokeDispatcher(OnLog, DispatcherPriority.Background, message);

            else
            {
                var component = _viewModel.GetLog(message);

                // New Log
                if (component == null)
                {
                    component = new LogComponentViewModel()
                    {
                        Name = message.GetLogName()
                    };

                    _viewModel.Logs.Add(component);
                }

                // Check Sub Log(s)
                var subLog = component.SubComponents.FirstOrDefault(x => x.Name == message.GetSubLogName());

                // New Sublog
                if (subLog == null)
                {
                    subLog = new LogSubComponentViewModel()
                    {
                        Name = message.GetSubLogName()
                    };

                    component.SubComponents.Add(subLog);
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
            this.Logs.Clear();
        }

        protected override void InitializeWork(IAudioStationConfiguration configuration)
        {

        }
    }
}
