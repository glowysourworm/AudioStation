
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Event;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IOutputController))]
    public class OutputController : IOutputController
    {
        public const int MAX_LOG_SIZE = 1000;

        private readonly IIocEventAggregator _eventAggregator;

        List<LogMessage> _generalLog;
        List<LogMessage> _databaseLog;

        [IocImportingConstructor]
        public OutputController(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _generalLog = new List<LogMessage>();
            _databaseLog = new List<LogMessage>();
        }

        public void AddLog(LogMessage message)
        {
            switch (message.Type)
            {
                case LogMessageType.General:
                    _generalLog.Insert(0, message);

                    if (_generalLog.Count > MAX_LOG_SIZE)
                        _generalLog.RemoveAt(_generalLog.Count - 1);
                    break;
                case LogMessageType.Database:
                    _databaseLog.Insert(0, message);

                    if (_databaseLog.Count > MAX_LOG_SIZE)
                        _databaseLog.RemoveAt(_generalLog.Count - 1);
                    break;
                default:
                    break;
            }

            _eventAggregator.GetEvent<LogEvent>().Publish(message);
        }
        public void AddLog(string message, LogMessageType type)
        {
            var logMessage = new LogMessage(message, type);

            this.AddLog(logMessage);
        }
        public void AddLog(string message, LogMessageType type, LogLevel severity, params object[] parameters)
        {
            var logMessage = new LogMessage(string.Format(message, parameters), type, severity);

            this.AddLog(logMessage);
        }

        public IEnumerable<LogMessage> GetLatestLogs(LogMessageType type, LogLevel level, int count)
        {
            switch (type)
            {
                case LogMessageType.General:
                    return _generalLog.Where(log => log.Level >= level).Take(count);
                case LogMessageType.Database:
                    return _databaseLog.Where(log => log.Level >= level).Take(count);
                default:
                    throw new Exception("Unhandled log message type:  OutputController.cs");
            }
        }

        public void ClearLogs(LogMessageType type)
        {
            switch (type)
            {
                case LogMessageType.General:
                    _generalLog.Clear();
                    break;
                case LogMessageType.Database:
                    _databaseLog.Clear();
                    break;
                default:
                    throw new Exception("Unhandled log message type:  OutputController.cs");
            }

            _eventAggregator.GetEvent<LogClearedEvent>().Publish(type);
        }

        public void Dispose()
        {
            _generalLog.Clear();
            _databaseLog.Clear();

            _generalLog = null;
            _databaseLog = null;
        }
    }
}
