using System.Collections.Concurrent;

using AudioStation.Core.Component;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Event;
using AudioStation.Core.Service.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Core.Controller
{
    [IocExport(typeof(IOutputController))]
    public class OutputController : IOutputController
    {
        public const int MAX_LOG_SIZE = 1000;

        private readonly IIocEventAggregator _eventAggregator;

        // Log message types have buckets here - one for each, with a max message count set in the 
        // constructor. (LibraryLoaderWorkItem logs should mostly be "specific"; but it's up to the user end)
        ConcurrentDictionary<int, LogComponent> _logs;

        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        private IAudioStationService.Status _status;

        [IocImportingConstructor]
        public OutputController(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _logs = new ConcurrentDictionary<int, LogComponent>();
        }

        #region (public) Log Methods
        public void Log(LogMessage message)
        {
            // Re-create message using our validation (and to keep a single implementation function)
            LogImpl(message.Message, message.Level, message.Type, message.ComponentType, message.ServiceType, message.DatabaseType, message.Exception);
        }

        public void Log(string message, LogMessageType type = LogMessageType.General)
        {
            Log(message, LogLevel.Information, type, null, Enumerable.Empty<object>());
        }
        public void Log(string message, LogMessageType type = LogMessageType.General, params object[] parameters)
        {
            Log(message, LogLevel.Information, type, null, parameters);
        }
        public void Log(string message, LogMessageComponentType componentType)
        {
            Log(message, componentType, LogLevel.Information, null, Enumerable.Empty<object>());
        }
        public void Log(string message, LogMessageServiceType serviceType)
        {
            Log(message, serviceType, LogLevel.Information, null, Enumerable.Empty<object>());
        }
        public void Log(string message, LogMessageDbType dbType)
        {
            Log(message, dbType, LogLevel.Information, null, Enumerable.Empty<object>());
        }

        public void Log(string message, LogLevel level, LogMessageType type = LogMessageType.General, Exception? exception = null)
        {
            Log(message, level, type, exception, Enumerable.Empty<object>());
        }
        public void Log(string message, LogMessageComponentType componentType, LogLevel level, Exception? exception = null)
        {
            Log(message, componentType, level, exception, Enumerable.Empty<object>());
        }
        public void Log(string message, LogMessageServiceType serviceType, LogLevel level, Exception? exception = null)
        {
            Log(message, serviceType, level, exception, Enumerable.Empty<object>());
        }
        public void Log(string message, LogMessageDbType dbType, LogLevel level, Exception? exception = null)
        {
            Log(message, dbType, level, exception, Enumerable.Empty<object>());
        }


        public void Log(string message, LogLevel level, LogMessageType type, Exception? exception, params object[] parameters)
        {
            switch (type)
            {
                case LogMessageType.General:
                case LogMessageType.OtherComponent:
                    LogImpl(message, level, type, LogMessageComponentType.None, LogMessageServiceType.None, LogMessageDbType.None, exception, parameters);
                    break;
                case LogMessageType.Component:
                case LogMessageType.Service:
                case LogMessageType.Database:
                    throw new Exception("Unspecified log message sub-type");
                default:
                    throw new Exception("Unhandled log message type");
            }
        }
        public void Log(string message, LogMessageComponentType componentType, LogLevel level, Exception? exception, params object[] parameters)
        {
            LogImpl(message, level, LogMessageType.Component, componentType, LogMessageServiceType.None, LogMessageDbType.None, exception, parameters);
        }
        public void Log(string message, LogMessageServiceType serviceType, LogLevel level, Exception? exception, params object[] parameters)
        {
            LogImpl(message, level, LogMessageType.Service, LogMessageComponentType.None, serviceType, LogMessageDbType.None, exception, parameters);
        }
        public void Log(string message, LogMessageDbType dbType, LogLevel level, Exception? exception, params object[] parameters)
        {
            LogImpl(message, level, LogMessageType.Database, LogMessageComponentType.None, LogMessageServiceType.None, dbType, exception, parameters);
        }
        #endregion

        private void LogImpl(string message,
                             LogLevel level,
                             LogMessageType type,
                             LogMessageComponentType componentType,
                             LogMessageServiceType serviceType,
                             LogMessageDbType dbType,
                             Exception? exception = null,
                             params object[] parameters)
        {
            var formattedMessage = message;

            // Validate
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message body of log message must be filled out");

            // Format
            if (parameters != null &&
                parameters.Any())
                formattedMessage = string.Format(message, parameters);

            // Full Constructor (with argument checks)
            var logMessage = new LogMessage(formattedMessage, type, componentType, serviceType, dbType, level, exception);

            // Log Hash Key
            var logKey = GetLogKey(logMessage);

            // New Log (type)
            if (!_logs.ContainsKey(logKey))
                _logs.AddOrUpdate(logKey, new LogComponent(MAX_LOG_SIZE), (hash, log) =>
                {
                    // TODO: THIS IS JUST A LOG.. ANY CONTENTIONS ARE VERY RARE. WE'LL COME BACK FOR THIS
                    return log;
                });

            // Add Log
            _logs[logKey].Add(logMessage);

            _eventAggregator.GetEvent<LogEvent>().Publish(logMessage);
        }

        public IEnumerable<LogMessage> GetLatestLogs(LogMessageType type,
                                                     LogMessageComponentType componentType,
                                                     LogMessageServiceType serviceType,
                                                     LogMessageDbType dbType,
                                                     LogLevel level,
                                                     int count)
        {
            var key = GetLogKey(type, componentType, serviceType, dbType);

            if (_logs.ContainsKey(key))
                return _logs[key].GetLatestLogs(level, count);

            else
                return Enumerable.Empty<LogMessage>();
        }
        public void ClearLogs(LogMessageType type)
        {
            var key = GetLogKey(type);

            if (_logs.ContainsKey(key))
            {
                _logs[key].Clear();
                _eventAggregator.GetEvent<LogClearedEvent>().Publish(type);
            }
        }

        public void Dispose()
        {
            _logs.Clear();
        }

        protected int GetLogKey(LogMessage message)
        {
            return GetLogKey(message.Type, message.ComponentType, message.ServiceType, message.DatabaseType);
        }
        protected int GetLogKey(LogMessageType type)
        {
            return GetLogKey(type, LogMessageComponentType.None);
        }
        protected int GetLogKey(LogMessageType type, LogMessageComponentType componentType)
        {
            return GetLogKey(type, componentType, LogMessageServiceType.None);
        }
        protected int GetLogKey(LogMessageType type, LogMessageComponentType componentType, LogMessageServiceType serviceType)
        {
            return GetLogKey(type, componentType, serviceType, LogMessageDbType.None);
        }
        protected int GetLogKey(LogMessageType type, LogMessageComponentType componentType, LogMessageServiceType serviceType, LogMessageDbType dbType)
        {
            return HashCode.Combine(type, componentType, serviceType, dbType);
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

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "Output Controller";
        }
        public string GetDisplayName()
        {
            return "Log";
        }
        public IAudioStationService.Status GetStatus()
        {
            // TODO: This status should maintain the "new log" / "logs viewed" status.

            return IAudioStationService.Status.Idle;
        }
        public async Task<IAudioStationService.Status> Initialize(AudioStationConfiguration configuration)
        {
            return IAudioStationService.Status.Idle;
        }
        public async Task<IAudioStationService.Status> ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationService.Status.Idle;
        }
        public string GetStatusMessage()
        {
            // TODO: This can break down logs by type; but we'll probably recomponentize later

            return string.Format("Log count:  {0}", _logs.Sum(x => x.Value.GetLogCount()));
        }
        #endregion
    }
}
