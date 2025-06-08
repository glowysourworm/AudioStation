using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Model;

using m3uParser;
using m3uParser.Model;

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

        public event SimpleEventHandler<LibraryEntry> LibraryEntryLoaded;
        public event SimpleEventHandler<RadioEntry> RadioEntryLoaded;
        public event SimpleEventHandler<LibraryLoaderWorkItem> WorkItemCompleted;
        public event SimpleEventHandler<LibraryLoaderWorkItem[]> WorkItemsAdded;
        public event SimpleEventHandler<LibraryLoaderWorkItem[]> WorkItemsRemoved;
        public event SimpleEventHandler ProcessingComplete;
        public event SimpleEventHandler<PlayStopPause, PlayStopPause> ProcessingChanged;

        private List<LibraryLoaderWorkItem> _workQueue;
        private Thread _workThread;
        private object _workThreadLock;
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
            _workThread.Priority = ThreadPriority.Lowest;
            _workThreadLock = new object();
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

            lock (_workThreadLock)
            {
                // COPY THESE - we're sending the rest to the main thread
                _workQueue.AddRange(workItems.Select(item => new LibraryLoaderWorkItem(item)));
            }

            // Fire event for new items in the queue
            if (this.WorkItemsAdded != null)
                this.WorkItemsAdded(workItems);
        }

        public LibraryEntry LoadLibraryEntry(string file, out TagLib.File fileRef)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("Invalid media file name");

            fileRef = null;

            try
            {
                fileRef = TagLib.File.Create(file);

                var entry = new LibraryEntry()
                {
                    FileName = file,
                    PrimaryArtist = Format(fileRef.Tag.FirstAlbumArtist),
                    PrimaryGenre = Format(fileRef.Tag.FirstGenre),

                    //AlbumArt = new SortedObservableCollection<SerializableBitmap>(fileRef.Tag.Pictures.Select(x => SerializableBitmap.ReadIPicture(x))),
                    Album = Format(fileRef.Tag.Album),
                    Disc = fileRef.Tag.Disc,
                    FileError = fileRef.PossiblyCorrupt,
                    FileErrorMessage = fileRef.CorruptionReasons?.Join(",", x => x) ?? string.Empty,
                    Title = Format(fileRef.Tag.Title),
                    Track = fileRef.Tag.Track
                };

                OnLog(string.Format("Music file loaded:  {0}", file), false);

                return entry;
            }
            catch (Exception ex)
            {
                var entry = new LibraryEntry()
                {
                    FileName = file,
                    FileError = true,
                    FileErrorMessage = ex.Message,        // Should be handled based on exception (user friendly message)
                };

                OnLog(string.Format("Music file load error:  {0}", file), true);

                return entry;
            }
        }

        public RadioEntry LoadRadioEntry(string fileName, out Extm3u m3uData)
        {
            m3uData = null;

            try
            {
                // Adding a nested try / catch for these files
                var fileContents = File.ReadAllText(fileName);
                m3uData = M3U.Parse(fileContents);

                var entry = new RadioEntry()
                {
                    Name = m3uData.Attributes.TvgName,                    
                    Streams = m3uData.Medias.Select(media => new RadioEntryStreamInfo()
                    {
                        Name = media.Title.RawTitle,
                        Homepage = media.Attributes.UrlTvg,
                        Endpoint = media.MediaFile,
                        LogoEndpoint = media.Attributes.TvgLogo
                    }).ToList()
                };

                OnLog(string.Format("Radio M3U file loaded:  {0}", fileName), false);

                return entry;
            }
            catch (Exception ex)
            {
                var entry = new RadioEntry()
                {
                    FileName = fileName,
                    FileError = true,
                    FileErrorMessage= ex.Message,
                };

                OnLog(string.Format("Radio M3U file load error:  {0}", fileName), true);

                return entry;
            }
        }

        private string Format(string tagField)
        {
            return string.IsNullOrWhiteSpace(tagField) ? string.Empty: tagField.Trim();
        }

        private void OnLog(string message, bool error)
        {
            // Shared between Dispatcher / our Worker Thread
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != _workThread.ManagedThreadId)
            {
                Application.Current.Dispatcher.BeginInvoke(OnLog, DispatcherPriority.ApplicationIdle, message, error);
                return;
            }

            _outputController.AddLog(message, LogMessageType.General, error ? LogLevel.Error : LogLevel.Information);
        }

        public PlayStopPause GetState()
        {
            lock(_workThreadLock)
            {
                return _userRun;
            }
        }

        public void Stop()
        {
            lock(_workThreadLock)
            {
                _userRun = PlayStopPause.Stop;
            }
        }

        public void Start()
        {
            lock (_workThreadLock)
            {
                _userRun = PlayStopPause.Play;
            }
        }

        public void Clear()
        {
            lock (_workThreadLock)
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

                lock (_workThreadLock)
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
                                TagLib.File tagRef = null;

                                var entry = LoadLibraryEntry(workItem.FileName, out tagRef);

                                // Set Work Item
                                if (entry.FileError)
                                {
                                    workItem.LoadState = LibraryWorkItemState.CompleteError;
                                    workItem.ErrorMessage = entry.FileErrorMessage;
                                }
                                else
                                {
                                    // Add to database
                                    _modelController.AddUpdateLibraryEntry(workItem.FileName, tagRef);

                                    workItem.LoadState = LibraryWorkItemState.CompleteSuccessful;
                                }

                                if (this.LibraryEntryLoaded != null)
                                    this.LibraryEntryLoaded(entry);
                            }
                            break;
                            case LibraryLoadType.M3UFile:
                            {
                                Extm3u m3uFile = null;

                                var entry = LoadRadioEntry(workItem.FileName, out m3uFile);

                                // Set Work Item
                                if (entry == null)
                                {
                                    workItem.LoadState = LibraryWorkItemState.CompleteError;
                                    workItem.ErrorMessage = "Error loading .m3u file:  " + workItem.FileName;
                                }
                                else
                                {
                                    // Add to database
                                    _modelController.AddRadioEntry(m3uFile);

                                    workItem.LoadState = LibraryWorkItemState.CompleteSuccessful;
                                }

                                if (this.RadioEntryLoaded != null)
                                    this.RadioEntryLoaded(entry);
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
                lock(_workThreadLock)
                {
                    _workerRun = false;
                }
                
                _workThread.Join(WORKER_SLEEP_PERIOD * 3);
                _workThread = null;
            }
        }
    }
}
