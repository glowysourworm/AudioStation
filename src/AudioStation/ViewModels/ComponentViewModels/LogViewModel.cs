using System.Windows.Threading;

using AudioStation.Core;
using AudioStation.Core.Event;
using AudioStation.Model;
using AudioStation.ViewModels.ComponentViewModels.LogViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.ComponentViewModels
{
    [IocExportDefault]
    public class LogViewModel : ComponentViewModelBase<LogSetViewModel>
    {
        public override LogSetViewModel? Load { get { return _viewModel; } }

        LogSetViewModel _viewModel;

        [IocImportingConstructor]
        public LogViewModel(IIocEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<LogEvent>().Subscribe(OnLog);
        }

        public override void Initialize(AudioStationConfiguration configuration, LogSetViewModel load, DialogProgressHandler progressHandler)
        {
            _viewModel = load;
        }

        private void OnLog(LogMessage message)
        {
            // During Initialization
            if (_viewModel == null)
                return;

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
        }
    }
}
