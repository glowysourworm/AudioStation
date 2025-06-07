using AudioStation.Model;

using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Responsible for maintaining / controlling log message and queue messages. We're going to 
    /// keep these in memory in-so-far as we need them. Should the work item queue get too big, we'll
    /// have to find other ways to manage it; but view binding is expected to be the biggest amount
    /// of memory. (so, please use virtual scrolling)
    /// </summary>
    public interface IOutputController : IDisposable
    {
        void AddLog(LogMessage message);
        void AddLog(string message, LogMessageType type);
        void AddLog(string message, LogMessageType type, LogLevel level, params object[] parameters);

        IEnumerable<LogMessage> GetLatestLogs(LogMessageType type, LogLevel level, int count);

        void ClearLogs(LogMessageType type);
    }
}
