using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public abstract class LibraryWorkerThreadBase : IDisposable
    {
        private const int THREAD_JOIN_WAIT = 1000;
        private const int THREAD_WAIT_SLEEP = 10;

        bool _run = true;
        bool _isWorking = false;
        Thread _thread;
        object _lock = new object();

        private readonly IOutputController _outputController;

        // Keep reference to the work item. This will be contended on with UI control.
        //
        private LibraryLoaderWorkItem _workItemIn;
        private LibraryLoaderWorkItem _workItemOut;

        public LibraryWorkerThreadBase(IOutputController outputController)
        {
            _outputController = outputController;

            _thread = new Thread(WorkDispatch);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.BelowNormal;
            _thread.Start();
        }

        /// <summary>
        /// Invokes logger on the application dispatcher thread
        /// </summary>
        protected void RaiseLog(int collectionId, string message, LogLevel level, params object[] parameters)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(RaiseLog, DispatcherPriority.Background, collectionId, message, level, parameters);

            else
                _outputController.LogSeparate(collectionId, message, LogMessageType.LibraryLoaderWorkItem, level, parameters);
        }

        /// <summary>
        /// Returns work item that was passed into the class for processing
        /// </summary>
        public LibraryLoaderWorkItem GetWorkResult()
        {
            lock (_lock)
            {
                return _workItemOut;
            }
        }

        public void ClearWorkItem()
        {
            lock(_lock)
            {
                if (_isWorking)
                    throw new Exception("Trying to clear work item while worker is still working");

                _workItemOut = null;
            }
        }

        public void SetWorkItem(LibraryLoaderWorkItem workItem)
        {
            lock (_lock)
            {
                if (_isWorking)
                    throw new Exception("Trying to set work item while worker is still working on the previous");

                if (workItem == null)
                    throw new ArgumentException("Cannot set work item to null. If you're trying to stop the thread, just use the Dispose() method");

                // Keep dispatcher's copy of the work item
                _workItemIn = workItem;

                // Let worker know it's time to work
                _isWorking = true;
            }
        }

        public bool IsReadyForWork()
        {
            lock(_lock)
            {
                return !_isWorking;
            }
        }

        /// <summary>
        /// Processes the actual work done by the thread
        /// </summary>
        protected abstract void Work(ref LibraryLoaderWorkItem workItem);

        private void WorkDispatch()
        {
            while (_run)
            {
                LibraryLoaderWorkItem workItem = null;

                // Copy work item onto the thread, make changes, then copy the data back.
                //
                lock (_lock)
                {
                    if (_workItemIn != null)
                    {
                        // <- In
                        workItem = new LibraryLoaderWorkItem(_workItemIn);

                        // Dispatcher will know we're working without having to wait on the lock
                        _isWorking = true;
                    }
                }

                // Process the work (also sets state information)
                if (workItem != null)
                    Work(ref workItem);

                // Notify that we're finished
                lock (_lock)
                {
                    if (workItem != null)
                    {
                        // -> Out
                        _workItemOut = new LibraryLoaderWorkItem(workItem);

                        // Dispatcher will know we're working without having to wait on the lock
                        _isWorking = false;
                    }
                }

                // Wait on dispatcher for next work item
                while (true && _run)
                {
                    Thread.Sleep(THREAD_WAIT_SLEEP);

                    lock(_lock)
                    {
                        if (_isWorking)
                            break;
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_thread != null)
            {
                lock(_lock)
                {
                    _run = false;
                }

                if (!_thread.Join(THREAD_JOIN_WAIT))
                    _thread.Abort();

                _thread = null;
            }
        }
    }
}
