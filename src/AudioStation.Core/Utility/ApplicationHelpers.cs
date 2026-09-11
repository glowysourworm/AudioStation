using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Model;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility.RecursiveComparer;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.Utilities;

namespace AudioStation.Core.Utility
{
    public static class ApplicationHelpers
    {
        private readonly static SimpleRecursiveComparer Comparer;

        static ApplicationHelpers()
        {
            Comparer = new SimpleRecursiveComparer();
        }

        private static IAudioStationLogService GetOutputController()
        {
            return IocContainer.Get<IAudioStationLogService>();
        }
        private static ILoggerFactory GetLoggerFactory()
        {
            return IocContainer.Get<ILoggerFactory>();
        }

        /// <summary>
        /// Sends a log request to the dispatcher to log with the output controller
        /// </summary>
        public static void Log(string message, params object[] parameters)
        {
            LogImpl(message, LogMessageType.General, LogLevel.Information, LogMessageComponentType.None, LogMessageServiceType.None, LogMessageDbType.None, null, parameters);
        }

        /// <summary>
        /// Sends a log request to the dispatcher to log with the output controller
        /// </summary>
        public static void Log(string message, LogLevel level, Exception? exception, params object[] parameters)
        {
            LogImpl(message, LogMessageType.General, level, LogMessageComponentType.None, LogMessageServiceType.None, LogMessageDbType.None, exception, parameters);
        }

        /// <summary>
        /// Sends a log request to the dispatcher to log with the output controller
        /// </summary>
        public static void Log(string message, LogMessageComponentType componentType, LogLevel level, Exception? exception, params object[] parameters)
        {
            LogImpl(message, LogMessageType.Component, level, componentType, LogMessageServiceType.None, LogMessageDbType.None, exception, parameters);
        }

        /// <summary>
        /// Sends a log request to the dispatcher to log with the output controller
        /// </summary>
        public static void Log(string message, LogMessageServiceType serviceType, LogLevel level, Exception? exception, params object[] parameters)
        {
            LogImpl(message, LogMessageType.Service, level, LogMessageComponentType.None, serviceType, LogMessageDbType.None, exception, parameters);
        }

        /// <summary>
        /// Sends a log request to the dispatcher to log with the output controller
        /// </summary>
        public static void Log(string message, LogMessageDbType dbType, LogLevel level, Exception? exception, params object[] parameters)
        {
            LogImpl(message, LogMessageType.Database, level, LogMessageComponentType.None, LogMessageServiceType.None, dbType, exception, parameters);
        }

        private static void LogImpl(string message,
                                    LogMessageType type,
                                    LogLevel level,
                                    LogMessageComponentType componentType,
                                    LogMessageServiceType serviceType,
                                    LogMessageDbType dbType,
                                    Exception? exception,
                                    params object[] parameters)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(LogImpl, DispatcherPriority.Background, message, type, level, componentType, serviceType, dbType, exception, parameters);

            else
            {
                switch (type)
                {
                    case LogMessageType.General:
                    case LogMessageType.OtherComponent:
                        GetOutputController().Log(message, level, type, exception, parameters);
                        break;
                    case LogMessageType.Component:
                        GetOutputController().Log(message, componentType, level, exception, parameters);
                        break;
                    case LogMessageType.Service:
                        GetOutputController().Log(message, serviceType, level, exception, parameters);
                        break;
                    case LogMessageType.Database:
                        GetOutputController().Log(message, dbType, level, exception, parameters);
                        break;

                    default:
                        throw new Exception("Unhandled log message type");
                }
            }
        }

        public static bool Compare<T>(T object1, T object2)
        {
            try
            {
                return Comparer.Compare<T>(object1, object2);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error comparing objects:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}
