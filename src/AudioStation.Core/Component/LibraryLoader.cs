using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent.Payload.Output;
using AudioStation.Core.Component.LibraryLoaderComponent.Worker;
using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Interface;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.Utilities;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(ILibraryLoader))]
    public class LibraryLoader : ILibraryLoader
    {
        private readonly IAudioStationMapper _audioStationMapper;
        private readonly IAudioStationFileService _fileController;
        private readonly IAudioStationDbClient _audioStationDbClient;
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IAudioConverter _audioConverter;
        private readonly ITagCache _tagCacheController;

        // Cannot use multi threading on the database until we have proper 
        // table locking, or transactions!

        public event SimpleEventHandler<LibraryLoaderWorkItemUpdate> WorkItemUpdate;
        public event SimpleEventHandler<LibraryLoaderWorkItem> WorkItemComplete;
        public event SimpleEventHandler<LibraryLoaderWorkItem> WorkItemQueued;
        public event SimpleEventHandler<LibraryLoaderWorkItem> WorkItemCanceled;
        public event SimpleEventHandler<PlayStopPause> StateChangeEvent;

        private Dictionary<int, LibraryLoaderWorkItem> _workQueue;
        private Dictionary<int, LibraryLoaderWorkItem> _workItemsWorking;
        private Dictionary<int, LibraryWorkerThreadBase> _workerThreads;

        // We're going to keep a history of the work items. An ID counter will supply id's to the
        // work items. In future cases, this may come from the database.
        //
        private int _workItemIdCounter;
        private PlayStopPause _loaderState;

        [IocImportingConstructor]
        public LibraryLoader(IAudioStationMapper audioStationMapper,
                             IAudioStationDbClient audioStationDbClient,
                             IAcoustIDClient acoustIDClient,
                             IMusicBrainzClient musicBrainzClient,
                             IAudioStationFileService fileController,
                             IAudioConverter audioConverter,
                             ITagCache tagCacheController)
        {
            _audioStationMapper = audioStationMapper;
            _audioStationDbClient = audioStationDbClient;
            _musicBrainzClient = musicBrainzClient;
            _acoustIDClient = acoustIDClient;
            _fileController = fileController;
            _audioConverter = audioConverter;
            _tagCacheController = tagCacheController;

            _workQueue = new Dictionary<int, LibraryLoaderWorkItem>();
            _workItemsWorking = new Dictionary<int, LibraryLoaderWorkItem>();
            _workerThreads = new Dictionary<int, LibraryWorkerThreadBase>();

            _workItemIdCounter = 0;
            _loaderState = PlayStopPause.Stop;
        }

        public int QueueLoaderTask(ILibraryLoaderLoad workLoad)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("ILibraryLoader must be accessed by the main thread");

            // NOTE:  The incremental work item ID property is a unique identifier! This must be maintained
            //        properly here by incremeting. It is used to identify logs for the task; and to have a
            //        handle for later querying.
            //
            LibraryLoaderWorkItem workItem = null;

            // This will create the work item with the proper output payload
            //
            switch (workLoad.LoadType)
            {
                case LibraryLoadType.AudioEncoding:
                {
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter, workLoad.OwnerId, LibraryLoadType.AudioEncoding);
                    workItem.Initialize(LibraryWorkItemState.Pending, workLoad,
                                        new LibraryLoaderOutput<LibraryLoaderAudioEncodingOutputPayload>(workLoad.LoadType,
                                        new LibraryLoaderAudioEncodingOutputPayload(), LibraryLoaderAudioEncodingWorker.GetNumberSteps()));
                }
                break;
                case LibraryLoadType.Import:
                {
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter, workLoad.OwnerId, LibraryLoadType.Import);
                    workItem.Initialize(LibraryWorkItemState.Pending, workLoad,
                                        new LibraryLoaderOutput<LibraryLoaderImportOutputPayload>(workLoad.LoadType,
                                        new LibraryLoaderImportOutputPayload(), LibraryLoaderImportWorker.GetNumberSteps()));
                }
                break;
                case LibraryLoadType.AcoustID:
                {
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter, workLoad.OwnerId, LibraryLoadType.AcoustID);
                    workItem.Initialize(LibraryWorkItemState.Pending, workLoad,
                                        new LibraryLoaderOutput<IList<AcoustIDLookupResult>>(workLoad.LoadType,
                                        new List<AcoustIDLookupResult>(), LibraryLoaderAcoustIDWorker.GetNumberSteps()));
                }
                break;
                case LibraryLoadType.MusicBrainzBasic:
                {
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter, workLoad.OwnerId, LibraryLoadType.MusicBrainzBasic);
                    workItem.Initialize(LibraryWorkItemState.Pending, workLoad,
                                        new LibraryLoaderOutput<LibraryLoaderMusicBrainzBasicOutputPayload>(workLoad.LoadType,
                                        new LibraryLoaderMusicBrainzBasicOutputPayload(), LibraryLoaderMusicBrainzBasicWorker.GetNumberSteps()));
                }
                break;
                case LibraryLoadType.MusicBrainzAlbumArt:
                {
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter, workLoad.OwnerId, LibraryLoadType.MusicBrainzAlbumArt);
                    workItem.Initialize(LibraryWorkItemState.Pending, workLoad,
                                        new LibraryLoaderOutput<FileReference>(workLoad.LoadType,
                                        new FileReference(), LibraryLoaderMusicBrainzAlbumArtWorker.GetNumberSteps()));
                }
                break;
                case LibraryLoadType.FileChecker:
                {
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter, workLoad.OwnerId, LibraryLoadType.FileChecker);
                    workItem.Initialize(LibraryWorkItemState.Pending, workLoad,
                                        new LibraryLoaderOutput<LibraryLoaderNoOutput>(workLoad.LoadType,
                                        new LibraryLoaderNoOutput(), LibraryLoaderFileCheckerWorker.GetNumberSteps()));
                }
                break;
                case LibraryLoadType.FileConverter:
                {
                    workItem = new LibraryLoaderWorkItem(_workItemIdCounter, workLoad.OwnerId, LibraryLoadType.FileConverter);
                    workItem.Initialize(LibraryWorkItemState.Pending, workLoad,
                                        new LibraryLoaderOutput<LibraryLoaderNoOutput>(workLoad.LoadType,
                                        new LibraryLoaderNoOutput(), LibraryLoaderFileConverterWorker.GetNumberSteps()));
                }
                break;
                case LibraryLoadType.ImportRadio:
                default:
                    throw new Exception("Unhandled library loader task type:  LibraryLoader.cs");
            }

            // Increment Work Counter
            _workItemIdCounter++;

            // Queue Work Item
            _workQueue.Add(workItem.GetId(), workItem);

            // Notify Listeners
            if (this.WorkItemQueued != null)
                this.WorkItemQueued(workItem);

            // -> State Change!
            OnStateChange(PlayStopPause.Play);

            CheckMoreWork();

            return workItem.GetId();
        }

        public bool IsWorkCompleted()
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("ILibraryLoader must be accessed by the main thread");

            return !_workerThreads.Any() && _workQueue.Count == 0;
        }

        public bool IsTaskQueued(int workItemId)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("ILibraryLoader must be accessed by the main thread");

            return _workQueue.ContainsKey(workItemId);
        }

        public bool IsTaskRunning(int workItemId)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("ILibraryLoader must be accessed by the main thread");

            if (!_workerThreads.ContainsKey(workItemId))
                return false;

            return _workerThreads[workItemId].GetExecutionState() == ThreadState.Running;
        }

        public void DequeueTask(int workItemId)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("ILibraryLoader must be accessed by the main thread");

            if (_workerThreads.ContainsKey(workItemId))
                throw new ArgumentException("Cannot dequeue task that is running. Must first cancel the task");

            if (!_workQueue.ContainsKey(workItemId))
                throw new ArgumentException("Work item is not queued. Please check before dequeuing.");

            // Work Item
            var workItem = _workQueue[workItemId];

            // Dequeue
            _workQueue.Remove(workItemId);

            // -> Cancel
            workItem.Update(LibraryWorkItemState.Canceled);

            if (this.WorkItemCanceled != null)
                this.WorkItemCanceled(workItem);
        }

        public void CancelTask(int workItemId)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("ILibraryLoader must be accessed by the main thread");

            if (!_workerThreads.ContainsKey(workItemId))
                throw new ArgumentException("Work item not found");

            // -> Join / Abort
            _workerThreads[workItemId].Stop();

            // -> GC
            _workerThreads.Remove(workItemId);

            if (_workItemsWorking.ContainsKey(workItemId))
            {
                // Work Item
                var workItem = _workItemsWorking[workItemId];

                // Remove from working
                _workItemsWorking.Remove(workItemId);

                // -> Cancel
                workItem.Update(LibraryWorkItemState.Canceled);

                if (this.WorkItemCanceled != null)
                    this.WorkItemCanceled(workItem);
            }

        }

        public void ChangeState(PlayStopPause state)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("ILibraryLoader must be accessed by the main thread");

            if (IsWorkCompleted())
                return;

            switch (state)
            {
                case PlayStopPause.Play:
                {
                    if (_loaderState == PlayStopPause.Play)
                        return;

                    OnStateChange(PlayStopPause.Play);

                    // -> Play ? (continue) : (halt)
                    CheckMoreWork();
                }
                break;
                case PlayStopPause.Pause:
                case PlayStopPause.Stop:
                {
                    if (_loaderState == state)
                        return;

                    OnStateChange(state);
                }
                break;
                default:
                    throw new Exception("Unhandled loader state request");
            }
        }

        /// <summary>
        /// Method that tends to thread "pool" and is used to start new threads or reuse existing threads
        /// for the next work item. This should be called when a thread exits; but we may not have a waiting
        /// thread pool / pattern (as of yet).
        /// </summary>
        private void CheckMoreWork()
        {
            // Next work item (1 THREAD ONLY!)  ^_^
            if (_workQueue.Count > 0 && _workerThreads.Count == 0 && _loaderState == PlayStopPause.Play)
            {
                // -> Dequeue
                var workItemId = _workQueue.Keys.Min();
                var workItem = _workQueue[workItemId];

                _workQueue.Remove(workItemId);

                // Next Thread
                LibraryWorkerThreadBase thread = null;

                switch (workItem.GetLoadType())
                {
                    case LibraryLoadType.AudioEncoding:
                    {
                        thread = new LibraryLoaderAudioEncodingWorker(_audioConverter, workItem);
                    }
                    break;
                    case LibraryLoadType.Import:
                    {
                        thread = new LibraryLoaderImportWorker(workItem, _audioStationDbClient, _fileController, _tagCacheController, _audioConverter);
                    }
                    break;
                    case LibraryLoadType.AcoustID:
                    {
                        thread = new LibraryLoaderAcoustIDWorker(_acoustIDClient, _audioStationDbClient, workItem);
                    }
                    break;
                    case LibraryLoadType.FileChecker:
                    {
                        thread = new LibraryLoaderFileCheckerWorker(_audioStationDbClient, workItem);
                    }
                    break;
                    case LibraryLoadType.FileConverter:
                    {
                        thread = new LibraryLoaderFileConverterWorker(_audioConverter, workItem);
                    }
                    break;
                    case LibraryLoadType.MusicBrainzBasic:
                    {
                        thread = new LibraryLoaderMusicBrainzBasicWorker(_audioStationMapper, _musicBrainzClient, _audioStationDbClient, workItem);
                    }
                    break;
                    case LibraryLoadType.MusicBrainzAlbumArt:
                    {
                        thread = new LibraryLoaderMusicBrainzAlbumArtWorker(_audioStationDbClient, _musicBrainzClient, _fileController, workItem);
                    }
                    break;
                    case LibraryLoadType.ImportRadio:
                    default:
                        throw new Exception("Unhandled work item type:  LibraryLoader.cs");
                }

                // -> Next Thread
                if (thread != null)
                {
                    // Make sure to hook / unhook these events before start / after complete
                    thread.ReportWorkStepStarted += Worker_ReportWorkStepStarted;
                    thread.ReportWorkStepComplete += Worker_ReportWorkStepComplete;
                    thread.ReportComplete += Worker_ReportComplete;

                    _workerThreads.Add(workItem.GetId(), thread);
                }

                // -> Working
                _workItemsWorking.Add(workItem.GetId(), workItem);

                // Start worker thread
                _workerThreads[workItem.GetId()].Start();
            }

            else if (_workQueue.Count == 0)
            {
                OnStateChange(PlayStopPause.Stop);
            }
        }

        private void CompleteWorker(LibraryWorkerThreadBase worker, LibraryLoaderWorkItem workItem)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                throw new Exception("Worker collections must be accessed by the main thread");

            // Remove worker from the list
            _workerThreads.Remove(workItem.GetId());

            // Add work item to the history
            _workItemsWorking.Remove(workItem.GetId());

            // Worker has reported complete. Go ahead and wait for a join.
            worker.Stop();
            worker = null;

            // Final Report Event
            if (this.WorkItemComplete != null)
                this.WorkItemComplete(workItem);

            CheckMoreWork();
        }

        private void OnStateChange(PlayStopPause state)
        {
            if (_loaderState != state)
            {
                _loaderState = state;

                if (this.StateChangeEvent != null)
                    this.StateChangeEvent(_loaderState);
            }
        }

        #region (private) Worker Thread Callbacks
        private void Worker_ReportComplete(LibraryWorkerThreadBase sender, LibraryLoaderWorkItem workItem)
        {
            // NOTE*** BeginInvoke must be allowing the worker (background thread) to exit its stack and finish the
            //         join. Otherwise, Thread.Abort is throwing a TargetOfInvocationException.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.BeginInvokeDispatcher(Worker_ReportComplete, DispatcherPriority.Background, sender, workItem);

            else
            {
                CompleteWorker(sender, workItem);
            }
        }
        private void Worker_ReportWorkStepComplete(LibraryWorkerThreadBase sender, LibraryLoaderWorkItemUpdate update)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Worker_ReportWorkStepComplete, DispatcherPriority.Background, sender, update);

            else if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
            {
                if (this.WorkItemUpdate != null)
                    this.WorkItemUpdate(update);
            }
        }
        private void Worker_ReportWorkStepStarted(LibraryWorkerThreadBase sender, LibraryLoaderWorkItemUpdate update)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Worker_ReportWorkStepStarted, DispatcherPriority.Background, sender, update);

            else if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.True)
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
                foreach (var worker in _workerThreads.Values)
                {
                    worker.Dispose();
                }

                _workerThreads.Clear();
                _workerThreads = null;
            }
        }
    }
}
