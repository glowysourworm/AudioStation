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

        [IocImportingConstructor]
        public OutputController(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _logs = new SimpleDictionary<LogMessageType, LogComponent>();

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
        public IEnumerable<LogMessage> GetLatestLogs(LogMessageType type, LogLevel level, int count)
        {
            return _logs[type].GetLatestLogs(type, level, count);
        }
        public void ClearLogs(LogMessageType type)
        {
            _logs[type].Clear();

            _eventAggregator.GetEvent<LogClearedEvent>().Publish(type);
        }

        public void Dispose()
        {
            _logs.Clear();
        }

        #region (public) ILogger (MSFT Design. This came about in two places:  AutoMapper, and Npgsql/EF database logging)
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // Lets add these to "OtherComponent" logs
            Log(formatter(state, exception), LogMessageType.OtherComponent);
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }
        #endregion
    }
}
