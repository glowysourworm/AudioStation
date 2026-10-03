using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Worker
{
    public abstract class LibraryWorkerThreadBase : IDisposable
    {
        private const int THREAD_JOIN_WAIT = 1000;

        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderWorkItemUpdate> ReportWorkStepStarted;
        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderWorkItemUpdate> ReportWorkStepComplete;
        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderWorkItem> ReportComplete;

        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderBulkWorkItemUpdate> BulkReportWorkStepStarted;
        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderBulkWorkItemUpdate> BulkReportWorkStepComplete;
        public event SimpleEventHandler<LibraryWorkerThreadBase, LibraryLoaderBulkWorkItem> BulkReportComplete;

        bool _loaded;
        Thread _thread;
        LibraryLoaderWorkItem _workItem;            // Unsafe! (follow it to see where it is used safely)
        LibraryLoaderBulkWorkItem _bulkWorkItem;    // Unsafe! (follow it to see where it is used safely)

        public LibraryWorkerThreadBase()
        {
            _loaded = false;
            _thread = null;
            _workItem = null;
            _bulkWorkItem = null;
        }

        public void LoadWorkItem(LibraryLoaderWorkItem workItem)
        {
            if (_loaded)
                throw new Exception("LibraryWorkerThreadBase is currently loaded. Please call Dispose() first.");

            _loaded = true;
            _workItem = workItem;
            _thread = new Thread(WorkDispatch);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.BelowNormal;
        }
        public void LoadWorkItem(LibraryLoaderBulkWorkItem bulkWorkItem)
        {
            if (_loaded)
                throw new Exception("LibraryWorkerThreadBase is currently loaded. Please call Dispose() first.");

            _loaded = true;
            _bulkWorkItem = bulkWorkItem;
            _thread = new Thread(BulkWorkDispatch);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.BelowNormal;
        }

        /// <summary>
        /// Processes the actual work done by the thread
        /// </summary>
        protected abstract bool WorkNext();

        /// <summary>
        /// Sends the work item to the derived class before execution. (see LoadWork() / Dispose() pattern)
        /// </summary>
        protected abstract void LoadWork(LibraryLoaderWorkItem workItem);

        /// <summary>
        /// Separate from Dispose() to unload the work item data from the worker. The Dispose() method overlaps
        /// the thread disposal.
        /// </summary>
        protected abstract void UnloadWork();

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
            // Set work item data for derived classes
            LoadWork(_workItem);

            // Update State
            _workItem.Start();

            // Processing of worker load is done in steps. Each thread instance is required
            // to give / maintain its step information for proper processing of the thread.
            // 

            // Process the work: We own the work item for a brief moment during steps. So, use events synchronously; and we may
            //                   send a report back to the UI.
            //
            while (GetCurrentWorkStep() != GetNumberOfWorkSteps())
            {
                if (this.ReportWorkStepStarted != null)
                    this.ReportWorkStepStarted(this, CreateUpdate(_workItem));

                var success = WorkNext();
                var finished = (GetCurrentWorkStep() == GetNumberOfWorkSteps());

                // Update State
                if (success)
                {
                    if (finished)
                        _workItem.Update(LibraryWorkItemState.Successful);
                }
                else
                {
                    _workItem.Update(LibraryWorkItemState.Error);
                }


                if (this.ReportWorkStepComplete != null)
                    this.ReportWorkStepComplete(this, CreateUpdate(_workItem));

                if (!success)
                    break;
            }

            if (this.ReportComplete != null)
                this.ReportComplete(this, _workItem);

            // Unloads work data in derived classes
            UnloadWork();
        }
        private void BulkWorkDispatch()
        {
            // Update State
            _bulkWorkItem.Start();

            // Processing of worker load is done in steps. Each thread instance is required
            // to give / maintain its step information for proper processing of the thread.
            // 
            while (_bulkWorkItem.GetCount(LibraryWorkItemState.Pending) > 0)
            {
                // Work Item
                //
                var workItem = _bulkWorkItem.Dequeue();

                // Set work item data for derived classes
                LoadWork(workItem);

                // Work Item Processing:  We don't want to send single update events. This is to avoid too much memory
                //                        and processing back and forth with the front end. We'll utilize bulk events
                //                        to report small amounts of data.
                //
                workItem.Start();

                // Bulk Report: We own the work item for a brief moment during steps. So, use events synchronously; and we may
                //              send a report back to the UI.
                //
                if (this.BulkReportWorkStepStarted != null)
                    this.BulkReportWorkStepStarted(this, CreateBulkUpdate(_bulkWorkItem));

                // Single Work Item:  The processing will follow a step-by-step processing. Then, we will call Reset() to
                //                    utilize the worker again.
                //
                while (GetCurrentWorkStep() != GetNumberOfWorkSteps())
                {
                    var success = WorkNext();
                    var finished = (GetCurrentWorkStep() == GetNumberOfWorkSteps());

                    // Update State
                    if (success)
                    {
                        if (finished)
                            workItem.Update(LibraryWorkItemState.Successful);
                    }
                    else
                    {
                        workItem.Update(LibraryWorkItemState.Error);
                    }

                    if (!success)
                        break;
                }

                // Unload work item data in derived classes
                UnloadWork();

                // Bulk Update:  Work Item "reports in" to the bulk item. This will alter the tallies which are send with
                //               the report event.
                //
                _bulkWorkItem.ReportComplete(workItem);

                // -> Bulk Report
                //
                if (this.BulkReportWorkStepComplete != null)
                    this.BulkReportWorkStepComplete(this, CreateBulkUpdate(_bulkWorkItem));
            }

            if (this.BulkReportComplete != null)
                this.BulkReportComplete(this, _bulkWorkItem);
        }

        private LibraryLoaderWorkItemUpdate CreateUpdate(LibraryLoaderWorkItem workItem)
        {
            return new LibraryLoaderWorkItemUpdate(workItem.GetId(),
                                            workItem.GetOwnerId(),
                                            workItem.GetLoadType(),
                                            workItem.GetOutputItem().ResultSteps,
                                            workItem.GetOutputItem().GetNumberOfSteps(),
                                            workItem.GetOutputItem().CurrentLog,
                                            workItem.GetLoadState());
        }
        private LibraryLoaderBulkWorkItemUpdate CreateBulkUpdate(LibraryLoaderBulkWorkItem bulkWorkItem)
        {
            return new LibraryLoaderBulkWorkItemUpdate(bulkWorkItem.GetId(), bulkWorkItem.GetOwnerId())
            {
                CompletedCount = bulkWorkItem.GetCompletedCount(),
                DataErrorCount = bulkWorkItem.GetCount(LibraryWorkerResultLevel.DataError),
                DataWarningCount = bulkWorkItem.GetCount(LibraryWorkerResultLevel.DataWarning),
                FailureCount = bulkWorkItem.GetCount(LibraryWorkerResultLevel.Failure),
                PendingCount = bulkWorkItem.GetCount(LibraryWorkItemState.Pending),
                ProcessingCount = bulkWorkItem.GetCount(LibraryWorkItemState.Processing),
                Progress = bulkWorkItem.GetProgress(),
                ServiceNoResultCount = bulkWorkItem.GetCount(LibraryWorkerResultLevel.ServiceNoResult),
                ServiceFailureCount = bulkWorkItem.GetCount(LibraryWorkerResultLevel.ServiceFailure),
                State = bulkWorkItem.GetLoadState(),
                SuccessCount = bulkWorkItem.GetCount(LibraryWorkerResultLevel.Success),
                TotalCount = bulkWorkItem.GetCount(),
                Type = bulkWorkItem.GetLoadType()
            };
        }

        public void Dispose()
        {
            // Loading:  Here we dispose of the thread; and set loaded = false. This
            //           will let the loader know we can accept a new load. The current
            //           load will be dereferenced when we receive a new one. 
            //

            if (_thread != null)
            {
                if (_thread.IsAlive)
                {
                    if (!_thread.Join(THREAD_JOIN_WAIT))
                        _thread.Abort();
                }

                _thread = null;
                _loaded = false;
            }
        }
    }
}
