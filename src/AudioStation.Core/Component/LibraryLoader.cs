using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model;
using AudioStation.Core.Model.M3U;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.NativeIO.FastDirectory;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(ILibraryLoader))]
    public class LibraryLoader : ILibraryLoader
    {
        private readonly IOutputController _outputController;
        private readonly IModelController _modelController;

        // Cannot use multi threading on the database until we have proper 
        // table locking, or transactions!

        const int WORKER_SLEEP_PERIOD = 500;                   // 1 second between queue checks
        const int WORKER_THREAD_MAX = 1;                       // This is actually per type because the work item is not classed out

        public event SimpleEventHandler WorkItemCompleted;
        public event SimpleEventHandler WorkItemUpdate;
        public event SimpleEventHandler WorkItemsAdded;
        public event SimpleEventHandler WorkItemsRemoved;
        public event SimpleEventHandler ProcessingComplete;
        public event SimpleEventHandler ProcessingChanged;

        private Queue<LibraryLoaderWorkItem> _workQueue;
        private List<LibraryWorkerThreadBase> _workerThreads;
        private Thread _workerDispatcher;
        private object _lock;
        private object _lockUserRun;
        private bool _run;
        private PlayStopPause _userRun;              // This is the flag to allow user to stop the queue
        private PlayStopPause _userRunLast;

        // Adds Id property to the work item for reference because of copying during processing
        private int _workItemIdCounter;

        [IocImportingConstructor]
        public LibraryLoader(IModelController modelController, IOutputController outputController)
        {
            _modelController = modelController;
            _outputController = outputController;

            _workQueue = new Queue<LibraryLoaderWorkItem>(); 
            _workerThreads = new List<LibraryWorkerThreadBase>();

            _workItemIdCounter = 0;
            _workerDispatcher = new Thread(WorkFunction);
            _workerDispatcher.Priority = ThreadPriority.BelowNormal;
            _lock = new object();
            _lockUserRun = new object();
            _run = true;

            _userRun = PlayStopPause.Stop;               // Queue must be started by user code
            _userRunLast = PlayStopPause.Stop;

            // Start worker thread (pool)
            for (int index = 0; index < WORKER_THREAD_MAX; index++)
            {
                _workerThreads.Add(new LibraryLoaderMp3AddUpdateWorker(modelController, outputController));
                _workerThreads.Add(new LibraryLoaderM3UAddUpdateWorker(modelController, outputController));
            }

            _workerDispatcher.Start();
        }

        public void LoadLibraryAsync(string baseDirectory)
        {
            LoadDirectoryAsync(baseDirectory, "*.mp3");
        }

        public void LoadRadioAsync(string baseDirectory)
        {
            LoadDirectoryAsync(baseDirectory, "*.m3u");
        }

        private void LoadDirectoryAsync(string baseDirectory, string searchPattern)
        {
            LibraryLoadType loadType;

            if (searchPattern == "*.mp3")
                loadType = LibraryLoadType.Mp3FileAddUpdate;

            else if (searchPattern == "*.m3u")
                loadType = LibraryLoadType.M3UFileAddUpdate;

            else
                throw new FormattedException("Unhandled search pattern type: {0}  LibraryLoader.cs", searchPattern);

            // Scan directories for files (Use NativeIO for much faster iteration. Less managed memory loading)
            var files = FastDirectoryEnumerator.GetFiles(baseDirectory, searchPattern, SearchOption.AllDirectories);
            var workItem = new LibraryLoaderWorkItem(_workItemIdCounter++, LibraryLoadType.Mp3FileAddUpdate);

            // Set initial state
            workItem.Initialize(LibraryWorkItemState.Pending, new LibraryLoaderFileLoad(files.Select(x => x.Path)));

            lock (_lock)
            {
                // Queue work item
                _workQueue.Enqueue(workItem);
            }

            // Fire event for new items in the queue
            if (this.WorkItemsAdded != null)
                this.WorkItemsAdded();
        }

        public PlayStopPause GetState()
        {
            lock (_lockUserRun)
            {
                return _userRun;
            }
        }

        public IEnumerable<LibraryWorkItem> GetWorkItems()
        {
            lock(_lock)     // Work Queue
            {
                var result = _workQueue.Select(x => new LibraryWorkItem()
                {
                    Id = x.GetId(),
                    LoadState = x.GetLoadState(),
                    LoadType = x.GetLoadType(),
                    PercentComplete = x.GetPercentComplete()

                }).ToList();

                return result;
            }
        }

        public void Stop()
        {
            lock (_lockUserRun)
            {
                _userRun = PlayStopPause.Stop;
            }
        }

        public void Start()
        {
            lock (_lockUserRun)
            {
                _userRun = PlayStopPause.Play;
            }
        }

        public void Clear()
        {
            lock(_lockUserRun)
            {
                _userRun = PlayStopPause.Stop;
            }

            lock (_lock)
            {
                // Our work items
                var workItems = _workQueue;

                // Clear the worker thread
                _workQueue.Clear();

                // -> Wait for Joins in the run loop. This will update the rest.
            }

            // Fire event to the UI
            if (this.WorkItemsRemoved != null)
                this.WorkItemsRemoved();
        }

        private void WorkFunction()
        {
            while (_run)
            {
                var workToProcess = false;
                var processingCompleted = false;

                // EVENTS:         The dispatcher invokes are meant to serialize the application's work to the UI. They're
                //                 needed for handoff of the work items.
                //
                //                 Also, the UI requests the User's Run Setting (on events). So, there needs to be a separation
                //                 from the locks to prevent a dead-lock.
                //
                // Primary Lock:   Work Queue (ONLY)
                // User Run Lock:  User Run Setting (events fired from this thread will contend with the primary lock)
                //

                PlayStopPause userRun;

                // User Run Lock
                lock(_lockUserRun)
                {
                    userRun = _userRun;
                }

                // Primary Lock (events content with user's run lock only)
                lock (_lock)
                {
                    if (_workQueue.Count > 0 && userRun == PlayStopPause.Play)
                    {
                        // Query for first work item
                        var workItem = _workQueue.Peek();

                        // Set this flag for sleep loop
                        workToProcess = true;

                        // Send work item to worker
                        switch (workItem.GetLoadType())
                        {
                            case LibraryLoadType.Mp3FileAddUpdate:
                            {
                                var worker = _workerThreads.FirstOrDefault(x => x.GetType() == typeof(LibraryLoaderMp3AddUpdateWorker) && x.IsReadyForWork());

                                // Starts Work (thread already running)
                                if (worker != null)
                                {
                                    // Check that we didn't miss last iteration (may have finished) (have to wait until this processes below)
                                    if (worker.GetWorkResult() == null)
                                    {
                                        worker.SetWorkItem(_workQueue.Dequeue());       // DEQUEUE
                                    }
                                }
                            }
                            break;
                            case LibraryLoadType.M3UFileAddUpdate:
                            {
                                var worker = _workerThreads.FirstOrDefault(x => x.GetType() == typeof(LibraryLoaderM3UAddUpdateWorker) && x.IsReadyForWork());

                                // Starts Work (thread already running)
                                if (worker != null)
                                {
                                    // Check that we didn't miss last iteration (may have finished) (have to wait until this processes below)
                                    if (worker.GetWorkResult() == null)
                                    {
                                        worker.SetWorkItem(_workQueue.Dequeue());       // DEQUEUE
                                    }
                                }
                            }
                            break;
                            default:
                                throw new Exception("Worker load type not handled:  LibraryLoader.cs");
                        }
                    }
                }

                // Manage Worker Threads (UI Events) (sent to dispatcher)
                foreach (var worker in _workerThreads)
                {
                    var item = worker.GetWorkResult();

                    // This thread is waiting for work (with a cleared item to indicate we handled the completion)
                    if (item == null)
                        continue;

                    // Updates sent to the Dispatcher
                    if (item.GetLoadState() == LibraryWorkItemState.CompleteSuccessful ||
                        item.GetLoadState() == LibraryWorkItemState.CompleteError)
                    {
                        // Notify UI
                        OnWorkItemCompleted(item);

                        // Clear Work Item (Ok to do here). There may be a stray extra loop for the current work
                        // item in case it barely had time to set the completed status. So, next iteration will
                        // send an extra event to the UI. Then, the work will be cleared.
                        //
                        if (worker.IsReadyForWork())
                            worker.ClearWorkItem();
                    }
                    else
                        OnWorkItemUpdate(item);
                }

                // User Run:  Process change of setting
                lock(_lockUserRun)
                {
                    // Auto-switch to turn off "Play" when processing is finished
                    //
                    switch (_userRun)
                    {
                        case PlayStopPause.Play:
                        {
                            // No work item -> Stop
                            if (!workToProcess)
                            {
                                _userRunLast = _userRun;
                                _userRun = PlayStopPause.Stop;
                                processingCompleted = true;
                            }
                        }
                        break;
                        case PlayStopPause.Pause:
                        case PlayStopPause.Stop:
                            break;
                        default:
                            throw new Exception("Unhandled loader state:  LibraryLoader.cs");
                    }
                }

                if (processingCompleted && this.ProcessingChanged != null)
                {
                    // -> Dispatcher -> Notify UI
                    //
                    OnProcessingChanged();
                }

                if (!workToProcess)
                {
                    Thread.Sleep(WORKER_SLEEP_PERIOD);
                }

                // Lets allow the main thread to catch up so we can see what's going on
                else
                    Thread.Sleep(5);
            }
        }

        /*
            Dispatcher:  Invoke is used to serialize the work to the dispatcher.
        */
        private void OnWorkItemCompleted(LibraryLoaderWorkItem workItem)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.Invoke(OnWorkItemCompleted, DispatcherPriority.Background, workItem);

            else
            {
                if (this.WorkItemCompleted != null)
                    this.WorkItemCompleted();
            }
        }
        private void OnWorkItemUpdate(LibraryLoaderWorkItem workItem)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.Invoke(OnWorkItemUpdate, DispatcherPriority.Background, workItem);

            else
            {
                if (this.WorkItemUpdate != null)
                    this.WorkItemUpdate();
            }
        }

        private void OnProcessingChanged()
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(OnProcessingChanged, DispatcherPriority.Background);

            else
            {
                if (this.ProcessingChanged != null)
                    this.ProcessingChanged();
            }
        }

        public void Dispose()
        {
            if (_workerDispatcher != null)
            {
                // May have to finish a final iteration (only a problem is there is work running)
                //
                lock (_lock)
                {
                    _run = false;
                }

                if (!_workerDispatcher.Join(WORKER_SLEEP_PERIOD * 3))
                    _workerDispatcher.Abort();

                _workerDispatcher = null;
            }
        }
    }
}
