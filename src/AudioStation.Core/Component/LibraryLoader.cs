using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
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

        const string UNKNOWN = "Unknown";
        const int WORKER_SLEEP_PERIOD = 500;                   // 1 second between queue checks

        public event SimpleEventHandler<LibraryLoaderWorkItem> WorkItemCompleted;
        public event SimpleEventHandler<LibraryLoaderWorkItem[]> WorkItemsAdded;
        public event SimpleEventHandler<LibraryLoaderWorkItem[]> WorkItemsRemoved;
        public event SimpleEventHandler ProcessingComplete;
        public event SimpleEventHandler<PlayStopPause, PlayStopPause> ProcessingChanged;

        private List<LibraryLoaderWorkItem> _workQueue;
        private Thread _workThread;
        private object _workerLock;
        private bool _workerRun;
        private PlayStopPause _userRun;              // This is the flag to allow user to stop the queue
        private PlayStopPause _userRunLast;

        [IocImportingConstructor]
        public LibraryLoader(IModelController modelController, IOutputController outputController)
        {
            _modelController = modelController;
            _outputController = outputController;

            _workQueue = new List<LibraryLoaderWorkItem>();
            _workThread = new Thread(WorkFunction);
            _workThread.Priority = ThreadPriority.Normal;
            _workerLock = new object();
            _workerRun = true;
            _userRun = PlayStopPause.Stop;               // Queue must be started by user code
            _userRunLast = PlayStopPause.Stop;
            _workThread.Start();
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
                loadType = LibraryLoadType.Mp3File;

            else if (searchPattern == "*.m3u")
                loadType = LibraryLoadType.M3UFile;

            else
                throw new FormattedException("Unhandled search pattern type: {0}  LibraryLoader.cs", searchPattern);

            // Scan directories for files (Use NativeIO for much faster iteration. Less managed memory loading)
            var files = FastDirectoryEnumerator.GetFiles(baseDirectory, searchPattern, SearchOption.AllDirectories);
            var workItems = new LibraryLoaderWorkItem[files.Length];
            var index = 0;

            foreach (var file in files)
            {
                workItems[index++] = new LibraryLoaderWorkItem(file.Path, loadType);
            }

            lock (_workerLock)
            {
                // COPY THESE - we're sending the rest to the main thread
                _workQueue.AddRange(workItems.Select(item => new LibraryLoaderWorkItem(item)));
            }

            // Fire event for new items in the queue
            if (this.WorkItemsAdded != null)
                this.WorkItemsAdded(workItems);
        }

        public TagLib.File LoadLibraryEntry(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("Invalid media file name");

            TagLib.File fileRef = null;

            try
            {
                fileRef = TagLib.File.Create(file);

                OnLog(string.Format("Music file loaded:  {0}", file), LogLevel.Information);

                return fileRef;
            }
            catch (Exception ex)
            {
                OnLog(string.Format("Music file load error:  {0}", file), LogLevel.Error);
            }

            return null;
        }

        public List<M3UStream> LoadRadioEntry(string fileName)
        {
            List<M3UStream> m3uData = null;

            try
            {
                // Adding a nested try / catch for these files
                m3uData = M3UParser.Parse(fileName, (no, op) => { } );

                // RadioEntry:  According to the M3U standard, a stream source must have a 
                //              duration setting of 0, or -1. We then should have a single
                //              media info. We can also add multiple with the same name; but
                //              there's really no reason to do this.
                //
                // Stream:      A streaming source will have at least one M3UMediaInfo entry;
                //              and this will have a duration of 0, or -1.
                //

                var validMedia = m3uData.Where(x => !string.IsNullOrEmpty(x.StreamSource) && 
                                                    !string.IsNullOrEmpty(x.Title))
                                        .DistinctBy(x => x.Title);

                if (validMedia.Any())
                    OnLog(string.Format("Radio M3U file loaded:  {0}", fileName), LogLevel.Information);

                return validMedia.ToList();
            }
            catch (Exception ex)
            {
                var entry = new RadioEntry()
                {
                    FileName = fileName,
                    FileError = true,
                    FileErrorMessage = ex.Message,
                };

                OnLog(string.Format("Radio M3U file load error:  {0}", fileName), LogLevel.Error);
            }

            return null;
        }

        private string Format(string tagField)
        {
            return string.IsNullOrWhiteSpace(tagField) ? string.Empty : tagField.Trim();
        }

        private void OnLog(string message, LogLevel level)
        {
            // Shared between Dispatcher / our Worker Thread
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
            {
                Application.Current.Dispatcher.BeginInvoke(OnLog, DispatcherPriority.ApplicationIdle, message, level);
                return;
            }

            _outputController.AddLog(message, LogMessageType.General, level);
        }

        public PlayStopPause GetState()
        {
            lock (_workerLock)
            {
                return _userRun;
            }
        }

        public void Stop()
        {
            lock (_workerLock)
            {
                _userRun = PlayStopPause.Stop;
            }
        }

        public void Start()
        {
            lock (_workerLock)
            {
                _userRun = PlayStopPause.Play;
            }
        }

        public void Clear()
        {
            lock (_workerLock)
            {
                _userRun = PlayStopPause.Stop;

                var workItems = _workQueue;

                // Clear the worker thread
                _workQueue.Clear();

                // Copy to the main thread (essentially)
                if (this.WorkItemsRemoved != null)
                    this.WorkItemsRemoved(workItems.ToArray());
            }
        }

        private void WorkFunction()
        {
            while (_workerRun)
            {
                var workToProcess = false;

                lock (_workerLock)
                {
                    // Query for first work item
                    var workItem = _workQueue.FirstOrDefault(item => item.LoadState == LibraryWorkItemState.Pending);

                    // Check for queue work (user has control over this portion from the front end)
                    if (workItem != null &&
                        _workQueue.Count > 0 &&
                        _userRun == PlayStopPause.Play)
                    {
                        // This state will not be needed unless we have processing that allows for transmission of 
                        // this to the main thread. Also, the processing will be done right here in a simple pass.
                        //
                        workItem.LoadState = LibraryWorkItemState.Processing;

                        // Set this flag for sleep loop
                        workToProcess = true;

                        switch (workItem.LoadType)
                        {
                            case LibraryLoadType.Mp3File:
                            {
                                var entry = LoadLibraryEntry(workItem.FileName);

                                // Set Work Item
                                if (entry == null || entry.PossiblyCorrupt)
                                {
                                    workItem.LoadState = LibraryWorkItemState.CompleteError;
                                    workItem.ErrorMessage = entry?.CorruptionReasons?.Join(",", x => x) ?? "Unknown Error";
                                }
                                else
                                {
                                    // Add to database
                                    _modelController.AddUpdateLibraryEntry(workItem.FileName, entry);

                                    workItem.LoadState = LibraryWorkItemState.CompleteSuccessful;
                                }
                            }
                            break;
                            case LibraryLoadType.M3UFile:
                            {
                                var streams = LoadRadioEntry(workItem.FileName);

                                // Set Work Item
                                if (streams == null || streams.Count == 0)
                                {
                                    workItem.LoadState = LibraryWorkItemState.CompleteError;
                                    workItem.ErrorMessage = "Error loading .m3u file:  " + workItem.FileName;
                                }
                                else
                                {
                                    // Add to database
                                    _modelController.AddRadioEntries(streams);

                                    workItem.LoadState = LibraryWorkItemState.CompleteSuccessful;
                                }
                            }
                            break;
                            default:
                                throw new Exception("Worker load type not handled:  LibraryLoader.cs");
                        }

                        // Notify listeners (work item completed)
                        if (this.WorkItemCompleted != null)
                            this.WorkItemCompleted(workItem);
                    }

                    // Auto-switch to turn off "Play" when processing is finished
                    switch (_userRun)
                    {
                        case PlayStopPause.Play:
                        {
                            // No work item -> Stop
                            if (!workToProcess)
                            {
                                _userRunLast = _userRun;
                                _userRun = PlayStopPause.Stop;
                            }
                        }
                        break;
                        case PlayStopPause.Pause:
                        case PlayStopPause.Stop:
                            break;
                        default:
                            throw new Exception("Unhandled loader state:  LibraryLoader.cs");
                    }

                    // EVENTS:  These would be safer to use outside the lock. We're contending other threads on
                    //          most private variables. The dispatcher invokes come back to wait on the next lock;
                    //          and may deadlock if they're not off the call stack (BeginInvoke)
                    //
                    if (_userRun != _userRunLast && this.ProcessingChanged != null)
                    {
                        this.ProcessingChanged(_userRunLast, _userRun);
                        _userRunLast = _userRun;
                    }
                }

                if (!workToProcess)
                {
                    Thread.Sleep(WORKER_SLEEP_PERIOD);
                }

                // Lets allow the main thread to catch up so we can see what's going on
                else
                    Thread.Sleep(1);
            }
        }

        public void Dispose()
        {
            if (_workThread != null)
            {
                // May have to finish a final iteration (only a problem is there is work running)
                //
                lock (_workerLock)
                {
                    _workerRun = false;
                }

                _workThread.Join(WORKER_SLEEP_PERIOD * 3);
                _workThread = null;
            }
        }
    }
}
