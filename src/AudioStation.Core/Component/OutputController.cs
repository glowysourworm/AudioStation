using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Model;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IOutputController))]
    public class OutputController : IOutputController
    {
        private readonly IIocEventAggregator _eventAggregator;

        List<LogMessage> _logMessages;
        List<LibraryLoaderWorkItem> _workItems;

        [IocImportingConstructor]
        public OutputController(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _logMessages = new List<LogMessage>();
            _workItems = new List<LibraryLoaderWorkItem>();
        }

        public void AddLog(LogMessage message)
        {
            _logMessages.Add(message);
            _eventAggregator.GetEvent<LogEvent>().Publish(message);
        }
        public void AddLog(string message)
        {
            var logMessage = new LogMessage(message);
            _logMessages.Add(logMessage);
            _eventAggregator.GetEvent<LogEvent>().Publish(logMessage);
        }
        public void AddLog(string message, LogMessageSeverity severity)
        {
            var logMessage = new LogMessage(message, severity);
            _logMessages.Add(logMessage);
            _eventAggregator.GetEvent<LogEvent>().Publish(logMessage);
        }
        public void AddLog(string message, LogMessageType type, LogMessageSeverity severity, params object[] parameters)
        {
            var logMessage = new LogMessage(string.Format(message, parameters), type, severity);
            _logMessages.Add(logMessage);
            _eventAggregator.GetEvent<LogEvent>().Publish(logMessage);
        }

        public void AddWorkItem(LibraryLoaderWorkItem workItem)
        {
            _workItems.Add(workItem);
            _eventAggregator.GetEvent<WorkItemEvent>().Publish(workItem);
        }

        public void ClearLogs()
        {
            _logMessages.Clear();
            _eventAggregator.GetEvent<WorkItemsClearedEvent>().Publish();
        }

        public void ClearWorkItems()
        {
            _workItems.Clear();
            _eventAggregator.GetEvent<LogClearedEvent>().Publish();
        }

        public void Dispose()
        {
            _logMessages.Clear();
            _workItems.Clear();

            _logMessages = null;
            _workItems = null;
        }
    }
}
