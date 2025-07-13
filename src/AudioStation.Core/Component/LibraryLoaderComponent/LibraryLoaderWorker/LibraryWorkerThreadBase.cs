using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker
{
    public abstract class LibraryWorkerThreadBase : IDisposable
    {
        private const int THREAD_JOIN_WAIT = 1000;

        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderWorkItemUpdate> ReportWorkStepStarted;
        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderWorkItemUpdate> ReportWorkStepComplete;
        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderWorkItem> ReportComplete;

        Thread _thread;
        LibraryLoaderWorkItem _workItem;    // Unsafe!

        public LibraryWorkerThreadBase(LibraryLoaderWorkItem workItem)
        {
            _workItem = workItem;
            _thread = new Thread(WorkDispatch);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.BelowNormal;
        }

        /// <summary>
        /// Processes the actual work done by the thread
        /// </summary>
        protected abstract bool WorkNext();

        public abstract int GetNumberOfWorkSteps();
        public abstract int GetCurrentWorkStep();

        public void Start()
        {
            if (_thread == null)
                throw new Exception("Thread has already been disposed");

            else if (_thread.IsAlive)
                throw new Exception("Thread has already been started");

            _thread.Start();
        }

        public void Stop()
        {
            if (_thread == null)
                throw new Exception("Thread has already been disposed");

            Dispose();

            if (_thread != null)
                throw new Exception("Thread disposal failed:  LibraryWorkerThreadBase.Stop()");
        }

        public ThreadState GetExecutionState()
        {
            if (_thread == null)
                throw new Exception("Thread has already been disposed");

            return _thread.ThreadState;
        }

        private void WorkDispatch()
        {
            // Processing of worker load is done in steps. Each thread instance is required
            // to give / maintain its step information for proper processing of the thread.
            // 

            // Process the work: We own the work item for a brief moment during steps. So, use events synchronously; and we may
            //                   send a report back to the UI.
            //
            while (GetCurrentWorkStep() != GetNumberOfWorkSteps())
            {
                if (this.ReportWorkStepStarted != null)
                    this.ReportWorkStepStarted(this, new LibraryLoaderWorkItemUpdate(_workItem.GetId(), _workItem.GetLoadType(), _workItem.GetOutputItem().Results, _workItem.GetOutputItem().Log, false));

                var success = WorkNext();
                var finished = (GetCurrentWorkStep() == GetNumberOfWorkSteps());

                if (this.ReportWorkStepComplete != null)
                    this.ReportWorkStepComplete(this, new LibraryLoaderWorkItemUpdate(_workItem.GetId(), _workItem.GetLoadType(), _workItem.GetOutputItem().Results, _workItem.GetOutputItem().Log, finished));

                if (!success)
                    break;
            }

            if (this.ReportComplete != null)
                this.ReportComplete(this, _workItem);
        }

        public void Dispose()
        {
            if (_thread != null)
            {
                if (_thread.IsAlive)
                {
                    if (!_thread.Join(THREAD_JOIN_WAIT))
                        _thread.Abort();
                }

                _thread = null;
            }
        }
    }
}
