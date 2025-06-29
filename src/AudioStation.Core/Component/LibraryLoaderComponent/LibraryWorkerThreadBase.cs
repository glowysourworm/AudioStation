using System.Windows;
using System.Windows.Threading;
using System.Xml;

using AudioStation.Core.Model;
using AudioStation.Core.Utility;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public abstract class LibraryWorkerThreadBase : IDisposable
    {
        private const int THREAD_JOIN_WAIT = 1000;
        private const int THREAD_WAIT_SLEEP = 10;

        public event SimpleEventHandler<LibraryWorkItem> LibraryWorkItemUpdate;

        bool _run = true;
        bool _isWorking = false;
        Thread _thread;
        object _isWorkingLock = new object();

        // Keep reference to the work item. This will be contended on with UI control.
        //
        private LibraryLoaderWorkItem _workItem;

        public LibraryWorkerThreadBase()
        {
            _thread = new Thread(WorkDispatch);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.BelowNormal;
            _thread.Start();
        }

        /// <summary>
        /// Processes the actual work done by the thread
        /// </summary>
        protected abstract void Work(ref LibraryLoaderWorkItem workItem);

        /// <summary>
        /// Reports on work item to the dispatcher thread via events
        /// </summary>
        protected void Report(LibraryWorkItem workItemData)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(Report, DispatcherPriority.Background, workItemData);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (this.LibraryWorkItemUpdate != null)
                    this.LibraryWorkItemUpdate(workItemData);
            }
        }

        public LibraryLoaderWorkItem ClearWorkItem()
        {
            lock (_isWorkingLock)
            {
                if (_isWorking)
                    throw new Exception("Trying to clear work item while worker is still working");
            }

            // Send back the only handle to the dispatching thread
            var result = _workItem;

            // Our reference is now null
            _workItem = null;

            return result;
        }

        public void SetWorkItem(LibraryLoaderWorkItem workItem)
        {
            lock (_isWorkingLock)
            {
                if (_isWorking)
                    throw new Exception("Trying to set work item while worker is still working on the previous");
            }

            if (workItem == null)
                throw new ArgumentException("Cannot set work item to null. If you're trying to stop the thread, just use the Dispose() method");

            // Keep dispatcher's copy of the work item
            _workItem = workItem;

            lock (_isWorkingLock)
            {
                // Let worker know it's time to work
                _isWorking = true;
            }
        }

        public bool IsReadyForWork()
        {
            lock (_isWorkingLock)
            {
                return !_isWorking;
            }
        }

        private void WorkDispatch()
        {
            while (_run)
            {
                // LOCK:  The long running process has no way to provide any updates except for an
                //        event passed back on its update cycle. This will be forwarded to the 
                //        dispatcher; and contain a copy of some of the work item's data.

                // Process the work (also sets state information)
                if (_workItem != null)
                    Work(ref _workItem);

                // Notify that we're finished
                lock (_isWorkingLock)
                {
                    // Send Report (before opening up the lock) (if work was completed this pass)
                    if (_workItem != null)
                    {
                        Report(new LibraryWorkItem()
                        {
                            Id = _workItem.GetId(),
                            HasErrors = _workItem.GetHasErrors(),
                            LastMessage = "Work Completed",
                            LoadState = _workItem.GetLoadState(),
                            LoadType = _workItem.GetLoadType(),
                            FailureCount = _workItem.GetFailureCount(),
                            SuccessCount = _workItem.GetSuccessCount(),
                            EstimatedCompletionTime = DateTime.Now.AddSeconds(DateTime.Now.Subtract(_workItem.GetStartTime()).TotalSeconds / (_workItem.GetPercentComplete() == 0 ? 1 : _workItem.GetPercentComplete())),
                            PercentComplete = _workItem.GetPercentComplete()
                        });
                    }

                    // Dispatcher will know we're working without having to wait on the lock
                    _isWorking = false;
                }

                // Wait on dispatcher for next work item
                while (true && _run)
                {
                    Thread.Sleep(THREAD_WAIT_SLEEP);

                    lock (_isWorkingLock)
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
                _run = false;

                if (!_thread.Join(THREAD_JOIN_WAIT))
                    _thread.Abort();

                _thread = null;
            }
        }
    }
}
