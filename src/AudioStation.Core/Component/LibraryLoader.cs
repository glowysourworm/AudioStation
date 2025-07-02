using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Utility;

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
        private readonly IModelController _modelController;

        // Cannot use multi threading on the database until we have proper 
        // table locking, or transactions!

        const int WORKER_SLEEP_PERIOD = 500;                   // 1 second between queue checks
        const int WORKER_THREAD_MAX = 1;                       // This is actually per type because the work item is not classed out

        public event SimpleEventHandler<LibraryWorkItem> WorkItemUpdate;
        public event SimpleEventHandler ProcessingUpdate;

        private Queue<LibraryLoaderWorkItem> _workQueue;
        private List<LibraryLoaderWorkItem> _workItemHistory;
        private List<LibraryWorkerThreadBase> _workerThreads;
        private Thread _workerDispatcher;
        private object _lock;
        private object _lockUserRun;
        private bool _run;
        private PlayStopPause _userRun;              // This is the flag to allow user to stop the queue

        // Adds Id property to the work item for reference because of copying during processing
        private int _workItemIdCounter;

        [IocImportingConstructor]
        public LibraryLoader(IModelController modelController, 
                             IMusicBrainzClient musicBrainzClient,
                             IAcoustIDClient acoustIDClient)
        {
            _modelController = modelController;

            _workQueue = new Queue<LibraryLoaderWorkItem>();
            _workItemHistory = new List<LibraryLoaderWorkItem>();
            _workerThreads = new List<LibraryWorkerThreadBase>();

            _workItemIdCounter = 0;
            _workerDispatcher = new Thread(WorkFunction);
            _workerDispatcher.Priority = ThreadPriority.BelowNormal;
            _lock = new object();
            _lockUserRun = new object();
            _run = true;

            _userRun = PlayStopPause.Stop;               // Queue must be started by user code

            // Start worker thread (pool)
            for (int index = 0; index < WORKER_THREAD_MAX; index++)
            {
                _workerThreads.Add(new LibraryLoaderMp3AddUpdateWorker(modelController));
                _workerThreads.Add(new LibraryLoaderM3UAddUpdateWorker(modelController));
                _workerThreads.Add(new LibraryLoaderFillMusicBrainzIdsWorker(modelController, musicBrainzClient));
                _workerThreads.Add(new LibraryLoaderImportStagedFilesWorker(modelController, acoustIDClient, musicBrainzClient));
            }
            foreach (var workerThread in _workerThreads)
            {
                workerThread.LibraryWorkItemUpdate += WorkerThread_LibraryWorkItemUpdate;
            }

            _workerDispatcher.Start();
        }

        public void RunLoaderTask(LibraryLoaderParameters parameters)
        {
            // NOTE:  The incremental work item ID property is a unique identifier! This must be maintained
            //        properly here by incremeting. It is used to identify logs for the task; and to have a
            //        handle for later querying.
            //
            LibraryLoaderWorkItem workItem = null;

            switch (parameters.LoadType)
            {
                case LibraryLoadType.LoadMp3FileData:
                {
                    var fileLoad = CreateFileLoad(parameters.SourceDirectory, "*.mp3");
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter++, LibraryLoadType.LoadMp3FileData);
                    workItem.Initialize(LibraryWorkItemState.Pending, fileLoad);
                }
                break;
                case LibraryLoadType.LoadM3UFileData:
                {
                    var fileLoad = CreateFileLoad(parameters.SourceDirectory, "*.m3u");
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter++, LibraryLoadType.LoadM3UFileData);
                    workItem.Initialize(LibraryWorkItemState.Pending, fileLoad);
                }
                break;
                case LibraryLoadType.FillMusicBrainzIds:
                {
                    var entityLoad = CreateAudioStationEntityLoad<Mp3FileReference>();
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter++, LibraryLoadType.FillMusicBrainzIds);
                    workItem.Initialize(LibraryWorkItemState.Pending, entityLoad);
                }
                break;
                case LibraryLoadType.ImportStagedFiles:
                {
                    var fileLoad = CreateFileLoad(parameters.SourceDirectory, "*.mp3");
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter++, LibraryLoadType.ImportStagedFiles);
                    workItem.Initialize(LibraryWorkItemState.Pending, fileLoad);
                }
                break;
                case LibraryLoadType.ImportRadioFiles:
                {
                    var fileLoad = CreateFileLoad(parameters.SourceDirectory, "*.m3u");
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter++, LibraryLoadType.ImportRadioFiles);
                    workItem.Initialize(LibraryWorkItemState.Pending, fileLoad);
                }
                break;
                default:
                    throw new Exception("Unhandled library loader task type:  LibraryLoader.cs");
            }

            // Queue Work Item
            lock(_lock)
            {
                _workQueue.Enqueue(workItem);
            }
        }

        private LibraryLoaderEntityLoad CreateAudioStationEntityLoad<TEntity>() where TEntity : AudioStationEntityBase
        {
            // Query for all music entity meta-data
            var entities = _modelController.GetAudioStationEntities<TEntity>();

            // Create work item for the music brainz search(es)
            return new LibraryLoaderEntityLoad(entities);
        }

        private LibraryLoaderFileLoad CreateFileLoad(string baseDirectory, string searchPattern)
        {
            // Scan directories for files (Use NativeIO for much faster iteration. Less managed memory loading)
            var files = FastDirectoryEnumerator.GetFiles(baseDirectory, searchPattern, SearchOption.AllDirectories);

            // Create the file load for the next work item
            return new LibraryLoaderFileLoad(files.Select(x => x.Path));
        }

        public PlayStopPause GetState()
        {
            lock (_lockUserRun)
            {
                return _userRun;
            }
        }

        public IEnumerable<LibraryWorkItem> GetIdleWorkItems()
        {
            lock (_lock)     // Work Queue, Work Item History
            {
                var result = _workQueue.Select(x => new LibraryWorkItem()
                {
                    Id = x.GetId(),
                    LoadState = x.GetLoadState(),
                    LoadType = x.GetLoadType(),
                    PercentComplete = x.GetPercentComplete(),
                    LastMessage = "Idle"

                }).ToList();

                result.AddRange(_workItemHistory.Select(x => new LibraryWorkItem()
                {
                    Id = x.GetId(),
                    LoadState = x.GetLoadState(),
                    LoadType = x.GetLoadType(),
                    PercentComplete = x.GetPercentComplete(),
                    LastMessage = "Idle"
                }));

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
            lock (_lockUserRun)
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
            OnProcessingUpdate();
        }

        private void WorkFunction()
        {
            while (_run)
            {
                var workToProcess = false;
                var workHistoryModified = false;

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
                lock (_lockUserRun)
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

                        // Send work item to worker
                        switch (workItem.GetLoadType())
                        {
                            case LibraryLoadType.LoadMp3FileData:
                            {
                                var worker = _workerThreads.FirstOrDefault(x => x.GetType() == typeof(LibraryLoaderMp3AddUpdateWorker) && x.IsReadyForWork());

                                // Starts Work (thread already running)
                                if (worker != null)
                                {
                                    worker.SetWorkItem(_workQueue.Dequeue());       // DEQUEUE
                                }
                            }
                            break;
                            case LibraryLoadType.LoadM3UFileData:
                            {
                                var worker = _workerThreads.FirstOrDefault(x => x.GetType() == typeof(LibraryLoaderM3UAddUpdateWorker) && x.IsReadyForWork());

                                // Starts Work (thread already running)
                                if (worker != null)
                                {
                                    worker.SetWorkItem(_workQueue.Dequeue());       // DEQUEUE
                                }
                            }
                            break;
                            case LibraryLoadType.FillMusicBrainzIds:
                            {
                                var worker = _workerThreads.FirstOrDefault(x => x.GetType() == typeof(LibraryLoaderFillMusicBrainzIdsWorker) && x.IsReadyForWork());

                                // Starts Work (thread already running)
                                if (worker != null)
                                {
                                    worker.SetWorkItem(_workQueue.Dequeue());       // DEQUEUE
                                }
                            }
                            break;
                            case LibraryLoadType.ImportStagedFiles:
                            {
                                var worker = _workerThreads.FirstOrDefault(x => x.GetType() == typeof(LibraryLoaderImportStagedFilesWorker) && x.IsReadyForWork());

                                // Starts Work (thread already running)
                                if (worker != null)
                                {
                                    worker.SetWorkItem(_workQueue.Dequeue());       // DEQUEUE
                                }
                            }
                            break;
                            default:
                                throw new Exception("Worker load type not handled:  LibraryLoader.cs");
                        }
                    }

                    // Manage Worker Threads (UI Events) (sent to dispatcher)
                    foreach (var worker in _workerThreads)
                    {
                        // Thread is still working
                        if (!worker.IsReadyForWork())
                            continue;

                        // Clear Work Item (Ok to do here). There may be a stray extra loop for the current work
                        // item in case it barely had time to set the completed status. So, next iteration will
                        // send an extra event to the UI. Then, the work will be cleared.
                        //
                        if (worker.IsReadyForWork())
                        {
                            // Set the thread's reference to null (pass the ball back)
                            var workItem = worker.ClearWorkItem();

                            // PROBLEM: No way to know whether work item is null already! This is because of our
                            //          long running worker threads... It's fine as long as we're done. The workers
                            //          could be more granular in their work processing.

                            // Store work item in our history
                            if (workItem != null)
                            {
                                _workItemHistory.Add(workItem);
                                workHistoryModified = true;
                            }
                        }
                    }

                    // Set this flag for sleep loop
                    workToProcess = _workQueue.Count > 0 || _workerThreads.Any(x => !x.IsReadyForWork());
                }

                if (!workToProcess && !workHistoryModified)
                {
                    Thread.Sleep(WORKER_SLEEP_PERIOD);
                }

                // Lets allow the main thread to catch up so we can see what's going on
                else
                {
                    Thread.Sleep(5);

                    // -> Dispatcher -> Notify UI
                    //
                    OnProcessingUpdate();
                }
            }
        }

        /*
            Dispatcher:  Invoke is used to serialize the work to the dispatcher.
        */
        private void OnProcessingUpdate()
        {
            if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                Application.Current.Dispatcher.BeginInvoke(OnProcessingUpdate, DispatcherPriority.Background);

            else if (ApplicationHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (this.ProcessingUpdate != null)
                    this.ProcessingUpdate();
            }
        }
        // This event actually occurrs on the UI dispatcher already
        private void WorkerThread_LibraryWorkItemUpdate(LibraryWorkItem sender)
        {
            if (this.WorkItemUpdate != null)
                this.WorkItemUpdate(sender);
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
