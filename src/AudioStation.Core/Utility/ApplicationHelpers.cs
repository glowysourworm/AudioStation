using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility.RecursiveComparer;
using AudioStation.Model;

using AutoMapper;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application;
using SimpleWpf.Native.IO;

namespace AudioStation.Core.Utility
{
    public static class ApplicationHelpers
    {
        private readonly static SimpleRecursiveComparer Comparer;

        static ApplicationHelpers()
        {
            Comparer = new SimpleRecursiveComparer();
        }

        private static IOutputController GetOutputController()
        {
            return IocContainer.Get<IOutputController>();
        }
        private static ILoggerFactory GetLoggerFactory()
        {
            return IocContainer.Get<ILoggerFactory>();
        }

        public static IEnumerable<string> FastGetFiles(string baseDirectory, string searchPattern, SearchOption option)
        {
            // Scan directories for files (Use NativeIO for much faster iteration. Less managed memory loading)
            using (var fastDirectory = new FastDirectoryIO(baseDirectory, searchPattern, option))
            {
                return fastDirectory.GetFiles()
                                    .Where(x => !x.IsDirectory)
                                    .Select(x => x.Path)
                                    .ToList();
            }
        }

        public static IEnumerable<FastDirectoryResult> FastGetFileData(string baseDirectory, string searchPattern, SearchOption option)
        {
            // Scan directories for files (Use NativeIO for much faster iteration. Less managed memory loading)
            using (var fastDirectory = new FastDirectoryIO(baseDirectory, searchPattern, option))
            {
                return fastDirectory.GetFiles()
                                    .Where(x => !x.IsDirectory)
                                    .ToList();
            }
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

        public static void BeginInvokeDispatcher(Delegate method, DispatcherPriority priority, params object[] parameters)
        {
            if (IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(method, priority, parameters);

            // Dispatcher (SYNCHRONOUS!)
            else
                method.DynamicInvoke(parameters);
        }

        public static async Task BeginInvokeDispatcherAsync(Delegate method, DispatcherPriority priority, params object[] parameters)
        {
            if (IsDispatcher() == ApplicationIsDispatcherResult.False)
                await Application.Current.Dispatcher.BeginInvoke(method, priority, parameters);

            // Dispatcher (SYNCHRONOUS!)
            else
                method.DynamicInvoke(parameters);
        }

        public static void InvokeDispatcher(Delegate method, DispatcherPriority priority, params object[] parameters)
        {
            if (IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.Invoke(method, priority, parameters);

            // Dispatcher
            else
                method.DynamicInvoke(parameters);
        }

        /// <summary>
        /// Sends a log request to the dispatcher to log with the output controller
        /// </summary>
        public static void Log(string message, LogMessageType type, LogLevel level, Exception? exception, params object[] parameters)
        {
            if (IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(Log, DispatcherPriority.Background, message, type, level, exception, parameters);

            else
                GetOutputController().Log(message, type, level, exception, parameters);
        }

        public static TDest Map<TSource, TDest>(TSource source)
        {
            try
            {
                var destination = Activator.CreateInstance(typeof(TDest));

                var mapper = GetMapper<TSource, TDest>();

                return (TDest)mapper.Map(source, destination, typeof(TSource), typeof(TDest));
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error mapping objects:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public static void MapOnto<TSource, TDest>(TSource source, TDest dest)
        {
            try
            {
                var mapper = GetMapper<TSource, TDest>();

                mapper.Map(source, dest, typeof(TSource), typeof(TDest));
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error mapping objects:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
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
                ApplicationHelpers.Log("Error comparing objects:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        private static IMapper GetMapper<TSource, TDest>()
        {
            if (ApplicationMapperCache.Has<TSource, TDest>())
                return ApplicationMapperCache.Get<TSource, TDest>();

            try
            {
                var config = new MapperConfiguration(cfg =>
                {
                    var map = cfg.CreateMap<TSource, TDest>();

                }, GetLoggerFactory());

                var mapper = config.CreateMapper();

                ApplicationMapperCache.Set<TSource, TDest>(mapper);

                return mapper;
            }
            catch (Exception ex)
            {
                Log("Error creating type mapper: {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}
