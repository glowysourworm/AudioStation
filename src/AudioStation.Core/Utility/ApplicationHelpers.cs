using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application;

namespace AudioStation.Core.Utility
{
    public static class ApplicationHelpers
    {
        private static IOutputController GetOutputController()
        {
            return IocContainer.Get<IOutputController>();
        }

        /// <summary>
        /// Checks to see whether the current managed thread is the dispatcher. Also, checks for application closing.
        /// </summary>
        public static ApplicationIsDispatcherResult IsDispatcher()
        {
            if (Application.Current == null)
                return ApplicationIsDispatcherResult.ApplicationClosing;

            else if (Thread.CurrentThread.ManagedThreadId == Application.Current.Dispatcher.Thread.ManagedThreadId)
                return ApplicationIsDispatcherResult.True;

            else
                return ApplicationIsDispatcherResult.False;
        }
        /// <summary>
        /// Sends a log request to the dispatcher to log with the output controller
        /// </summary>
        public static void Log(string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            if (IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(Log, DispatcherPriority.Background, message, type, level, parameters);

            else
                GetOutputController().Log(message, type, level, parameters);
        }

        /// <summary>
        /// Sends a log request to the dispatcher to log with the output controller
        /// </summary>
        public static void LogSeparate(int logId, string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            if (IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(LogSeparate, DispatcherPriority.Background, logId, message, type, level, parameters);

            else
                GetOutputController().LogSeparate(logId, message, type, level, parameters);
        }
    }
}
