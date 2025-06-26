using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IOutputController))]
    public class OutputController : IOutputController
    {
        public const int MAX_LOG_SIZE = 1000;

        private readonly IIocEventAggregator _eventAggregator;

        // Log message types have buckets here - one for each, with a max message count set in the 
        // constructor. (LibraryLoaderWorkItem logs should mostly be "specific"; but it's up to the user end)
        SimpleDictionary<LogMessageType, LogComponent> _logs;

        // The specific logs are mostly meant for the library loader. Each work item may have a log
        // with a specified Id so that the user can browse the log individually.
        //
        SimpleDictionary<int, LogComponent> _specificLogs;

        [IocImportingConstructor]
        public OutputController(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _logs = new SimpleDictionary<LogMessageType, LogComponent>();
            _specificLogs = new SimpleDictionary<int, LogComponent>();

            foreach (var value in Enum.GetValues(typeof(LogMessageType)))
            {
                _logs.Add((LogMessageType)value, new LogComponent(MAX_LOG_SIZE));
            }
        }

        public void Log(LogMessage message)
        {
            if (!_logs.ContainsKey(message.Type))
                _logs.Add(message.Type, new LogComponent(MAX_LOG_SIZE));

            _logs[message.Type].Add(message);
            _eventAggregator.GetEvent<LogEvent>().Publish(message);
        }
        public void Log(string message, LogMessageType type)
        {
            var logMessage = new LogMessage(message, type);

            this.Log(logMessage);
        }
        public void Log(string message, LogMessageType type, LogLevel severity, params object[] parameters)
        {
            var logMessage = new LogMessage(string.Format(message, parameters), type, severity);

            this.Log(logMessage);
        }
        public void LogSeparate(int collectionId, LogMessage message)
        {
            if (!_specificLogs.ContainsKey(collectionId))
                _specificLogs.Add(collectionId, new LogComponent(MAX_LOG_SIZE));

            _specificLogs[collectionId].Add(message);
            _eventAggregator.GetEvent<LogEvent>().Publish(message);
        }

        public void LogSeparate(int collectionId, string message, LogMessageType type)
        {
            var logMessage = new LogMessage(message, type);

            this.LogSeparate(collectionId, logMessage);
        }

        public void LogSeparate(int collectionId, string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            var logMessage = new LogMessage(string.Format(message, parameters), type, level);

            this.LogSeparate(collectionId, logMessage);
        }

        public IEnumerable<LogMessage> GetLatestSeparateLogs(int collectionId, LogMessageType type, LogLevel level, int count)
        {
            if (!_specificLogs.ContainsKey(collectionId))
                throw new ArgumentException("Separate log not yet made for collection (Id):  " + collectionId.ToString());

            return _specificLogs[collectionId].GetLatestLogs(type, level, count);
        }
        public IEnumerable<LogMessage> GetLatestLogs(LogMessageType type, LogLevel level, int count)
        {
            return _logs[type].GetLatestLogs(type, level, count);
        }
        public void ClearLogs(int collectionId, LogMessageType type)
        {
            _specificLogs[collectionId].Clear();

            _eventAggregator.GetEvent<LogClearedEvent>().Publish(type);
        }
        public void ClearLogs(LogMessageType type)
        {
            _logs[type].Clear();

            _eventAggregator.GetEvent<LogClearedEvent>().Publish(type);
        }

        public void Dispose()
        {
            _logs.Clear();
            _specificLogs.Clear();
        }
    }
}
