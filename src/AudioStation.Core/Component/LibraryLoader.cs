using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderWorker;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Utility;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(ILibraryLoader))]
    public class LibraryLoader : ILibraryLoader
    {
        private readonly ILibraryImporter _libraryImporter;
        private readonly IFileController _fileController;

        // Cannot use multi threading on the database until we have proper 
        // table locking, or transactions!

        public event SimpleEventHandler<LibraryLoaderWorkItemUpdate> WorkItemUpdate;
        public event SimpleEventHandler<LibraryLoaderOutputBase> WorkItemComplete;

        private Queue<LibraryLoaderWorkItem> _workQueue;
        private List<LibraryLoaderWorkItem> _workItemsWorking;
        private List<LibraryLoaderWorkItem> _workItemHistory;
        private List<LibraryWorkerThreadBase> _workerThreads;

        // We're going to keep a history of the work items. An ID counter will supply id's to the
        // work items. In future cases, this may come from the database.
        //
        private int _workItemIdCounter;

        [IocImportingConstructor]
        public LibraryLoader(ILibraryImporter libraryImporter,
                             IFileController fileController)
        {
            _libraryImporter = libraryImporter;
            _fileController = fileController;

            _workQueue = new Queue<LibraryLoaderWorkItem>();
            _workItemsWorking = new List<LibraryLoaderWorkItem>();
            _workItemHistory = new List<LibraryLoaderWorkItem>();
            _workerThreads = new List<LibraryWorkerThreadBase>();

            _workItemIdCounter = 0;
            
        }

        public void RunLoaderTaskAsync<TIn>(LibraryLoaderParameters<TIn> parameters) where TIn : LibraryLoaderLoadBase
        {
            // NOTE:  The incremental work item ID property is a unique identifier! This must be maintained
            //        properly here by incremeting. It is used to identify logs for the task; and to have a
            //        handle for later querying.
            //
            LibraryLoaderWorkItem workItem = null;

            switch (parameters.LoadType)
            {
                case LibraryLoadType.Import:
                {
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter++, LibraryLoadType.Import);
                    workItem.Initialize(LibraryWorkItemState.Pending, parameters.Load, new LibraryLoaderImportOutput());
                }
                break;
                case LibraryLoadType.ImportRadio:
                case LibraryLoadType.DownloadMusicBrainz:
                default:
                    throw new Exception("Unhandled library loader task type:  LibraryLoader.cs");
            }

            // Queue Work Item
            _workQueue.Enqueue(workItem);

            CheckMoreWork();
        }

        public bool IsWorkCompleted()
        {
            return !_workerThreads.Any() && _workQueue.Count == 0;
        }

        /// <summary>
        /// Method that tends to thread "pool" and is used to start new threads or reuse existing threads
        /// for the next work item. This should be called when a thread exits; but we may not have a waiting
        /// thread pool / pattern (as of yet).
        /// </summary>
        private void CheckMoreWork()
        {
            // Next work item
            if (_workQueue.Count > 0 && _workerThreads.Count == 0)
            {
                // -> Dequeue
                var workItem = _workQueue.Dequeue();

                switch (workItem.GetLoadType())
                {
                    case LibraryLoadType.Import:
                    {
                        var thread = new LibraryLoaderImportWorker(workItem, _libraryImporter);

                        // Make sure to hook / unhook these events before start / after complete
                        thread.ReportWorkStepStarted += Worker_ReportWorkStepStarted;
                        thread.ReportWorkStepComplete += Worker_ReportWorkStepComplete;
                        thread.ReportComplete += Worker_ReportComplete;

                        _workerThreads.Add(thread);
                    }
                    break;
                    case LibraryLoadType.ImportRadio:
                    case LibraryLoadType.DownloadMusicBrainz:
                    default:
                        throw new Exception("Unhandled work item type:  LibraryLoader.cs");
                }

                // -> Working
                _workItemsWorking.Add(workItem);

                // Start worker thread
                _workerThreads[_workerThreads.Count - 1].Start();
            }
        }

        private void CompleteWorker(LibraryWorkerThreadBase worker, LibraryLoaderWorkItem workItem)
        {
            // Remove worker from the list
            _workerThreads.Remove(worker);

            // Add work item to the history
            _workItemsWorking.Remove(workItem);
            _workItemHistory.Add(workItem);

            // Worker has reported complete. Go ahead and wait for a join.
            worker.Stop();
            worker = null;

            // Final Report Event
            if (this.WorkItemComplete != null)
                this.WorkItemComplete(workItem.GetOutputItem());

            CheckMoreWork();
        }

        #region (private) Worker Thread Callbacks
        private void Worker_ReportComplete(LibraryWorkerThreadBase sender, LibraryLoaderWorkItem workItem)
        {
            // NOTE*** BeginInvoke must be allowing the worker (background thread) to exit its stack and finish the
            //         join. Otherwise, Thread.Abort is throwing a TargetOfInvocationException.
            //
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                ApplicationHelpers.BeginInvokeDispatcher(Worker_ReportComplete, DispatcherPriority.Background, sender, workItem);

            else
            {
                CompleteWorker(sender, workItem);
            }
        }
        private void Worker_ReportWorkStepComplete(LibraryWorkerThreadBase sender, LibraryLoaderWorkItemUpdate update)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                ApplicationHelpers.InvokeDispatcher(Worker_ReportWorkStepComplete, DispatcherPriority.Background, sender, update);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (this.WorkItemUpdate != null)
                    this.WorkItemUpdate(update);
            }
        }
        private void Worker_ReportWorkStepStarted(LibraryWorkerThreadBase sender, LibraryLoaderWorkItemUpdate update)
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                ApplicationHelpers.InvokeDispatcher(Worker_ReportWorkStepStarted, DispatcherPriority.Background, sender, update);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (this.WorkItemUpdate != null)
                    this.WorkItemUpdate(update);
            }
        }
        #endregion

        public void Dispose()
        {
            if (_workerThreads != null)
            {
                foreach (var worker in _workerThreads)
                {
                    worker.Dispose();
                }

                _workerThreads.Clear();
                _workerThreads = null;
            }
        }
    }
}
