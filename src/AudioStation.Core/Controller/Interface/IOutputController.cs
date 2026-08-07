using AudioStation.Core.Service.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Controller.Interface
{
    /// <summary>
    /// Responsible for maintaining / controlling log message and queue messages. We're going to 
    /// keep these in memory in-so-far as we need them. Should the work item queue get too big, we'll
    /// have to find other ways to manage it; but view binding is expected to be the biggest amount
    /// of memory. (so, please use virtual scrolling)
    /// </summary>
    public interface IOutputController : ILogger, IDisposable, IAudioStationService
    {
        void Log(LogMessage message);
        void Log(string message, LogMessageType type = LogMessageType.General, params object[] parameters);
        void Log(string message, LogMessageType type = LogMessageType.General);
        void Log(string message, LogMessageComponentType componentType);
        void Log(string message, LogMessageServiceType serviceType);

        void Log(string message, LogMessageDbType dbType);
        void Log(string message, LogLevel level, LogMessageType type = LogMessageType.General, Exception? exception = null);
        void Log(string message, LogMessageComponentType componentType, LogLevel level, Exception? exception = null);
        void Log(string message, LogMessageServiceType serviceType, LogLevel level, Exception? exception = null);
        void Log(string message, LogMessageDbType dbType, LogLevel level, Exception? exception = null);

        void Log(string message, LogLevel level, LogMessageType type, Exception? exception, params object[] parameters);
        void Log(string message, LogMessageComponentType componentType, LogLevel level, Exception? exception, params object[] parameters);
        void Log(string message, LogMessageServiceType serviceType, LogLevel level, Exception? exception, params object[] parameters);
        void Log(string message, LogMessageDbType dbType, LogLevel level, Exception? exception, params object[] parameters);

        IEnumerable<LogMessage> GetLatestLogs(LogMessageType type,
                                              LogMessageComponentType componentType,
                                              LogMessageServiceType serviceType,
                                              LogMessageDbType dbType,
                                              LogLevel level,
                                              int count);

        void ClearLogs(LogMessageType type);
    }
}
