using System.IO;
using System.Windows;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Model;

using m3uParser;

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

        const string UNKNOWN = "Unknown";
        const int WORKER_SLEEP_PERIOD = 500;                   // 1 second between queue checks

        public event SimpleEventHandler<LibraryEntry> LibraryEntryLoaded;
        public event SimpleEventHandler<RadioEntry> RadioEntryLoaded;
        public event SimpleEventHandler<LibraryLoaderWorkItem> WorkItemCompleted;
        public event SimpleEventHandler<LibraryLoaderWorkItem[]> WorkItemsAdded;
        public event SimpleEventHandler<LibraryLoaderWorkItem[]> WorkItemsRemoved;
        public event SimpleEventHandler ProcessingComplete;
        public event SimpleEventHandler<PlayStopPause> ProcessingChanged;

        private List<LibraryLoaderWorkItem> _workQueue;
        private Thread _workThread;
        private object _workThreadLock;
        private bool _workerRun;
        private bool _userRun;              // This is the flag to allow user to stop the queue

        [IocImportingConstructor]
        public LibraryLoader(IOutputController outputController)
        {
            _outputController = outputController;

            _workQueue = new List<LibraryLoaderWorkItem>();
            _workThread = new Thread(WorkFunction);
            _workThreadLock = new object();
            _workerRun = true;
            _userRun = false;               // Queue must be started by user code
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

        public LibraryEntry LoadLibraryEntry(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("Invalid media file name");

            try
            {

                var fileRef = TagLib.File.Create(file);

                var entry = new LibraryEntry()
                {
                    FileName = file,
                    PrimaryArtist = fileRef.Tag.FirstAlbumArtist,
                    PrimaryGenre = fileRef.Tag.FirstGenre,

                    //AlbumArt = new SortedObservableCollection<SerializableBitmap>(fileRef.Tag.Pictures.Select(x => SerializableBitmap.ReadIPicture(x))),
                    Album = Format(fileRef.Tag.Album),
                    Disc = fileRef.Tag.Disc,
                    FileError = fileRef.PossiblyCorrupt,
                    FileErrorMessage = fileRef.CorruptionReasons.Join(",", x => x),
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

        public RadioEntry LoadRadioEntry(string fileName)
        {
            try
            {
                // Adding a nested try / catch for these files
                var fileContents = File.ReadAllText(fileName);
                var m3uFile = M3U.Parse(fileContents);

                var entry = new RadioEntry()
                {
                    Name = m3uFile.Attributes.TvgName,
                    Streams = m3uFile.Medias.Select(media => new RadioEntryStreamInfo()
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
                    FileErrorMessage = ex.Message,
                };

                OnLog(string.Format("Radio M3U file load error:  {0}", fileName), true);

                return entry;
            }
        }

        private string Format(string tagField)
        {
            return string.IsNullOrWhiteSpace(tagField) ? UNKNOWN : tagField;
        }

        private void OnLog(string message, bool error)
        {
            // Shared between Dispatcher / our Worker Thread
            if (Application.Current.Dispatcher.Thread.ManagedThreadId != _workThread.ManagedThreadId)
            {
                Application.Current.Dispatcher.BeginInvoke(OnLog);
                return;
            }

            _outputController.AddLog(message, LogMessageType.General, error ? LogLevel.Error : LogLevel.Information);
        }

        public PlayStopPause GetState()
        {
            lock(_workThreadLock)
            {
                if (_userRun && _workQueue.Count > 0)
                    return PlayStopPause.Play;

                else if (_workQueue.Count > 0)
                    return PlayStopPause.Pause;

                else
                    return PlayStopPause.Stop;
            }
            
        }

        public void Stop()
        {
            lock(_workThreadLock)
            {
                _userRun = false;
            }
        }

        public void Start()
        {
            lock (_workThreadLock)
            {
                _userRun = true;
            }
        }

        public void Clear()
        {
            lock (_workThreadLock)
            {
                _userRun = false;

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
                var previousState = GetState();      // ENTERS LOCK

                lock (_workThreadLock)
                {
                    // Check for queue work (user has control over this portion from the front end)
                    if (_workQueue.Count > 0 && _userRun)
                    {
                        var workItem = _workQueue.FirstOrDefault(item => item.LoadState == LibraryWorkItemState.Pending);

                        // This state will not be needed unless we have processing that allows for transmission of 
                        // this to the main thread. Also, the processing will be done right here in a simple pass.
                        //
                        workItem.LoadState = LibraryWorkItemState.Processing;

                        // DONE!
                        if (workItem.Equals(default(LibraryLoaderWorkItem)))
                        {
                            workToProcess = true;

                            switch (workItem.LoadType)
                            {
                                case LibraryLoadType.Mp3File:
                                {
                                    var entry = LoadLibraryEntry(workItem.FileName);

                                    // Set Work Item
                                    if (entry.FileError)
                                    {
                                        workItem.LoadState = LibraryWorkItemState.CompleteError;
                                        workItem.ErrorMessage = entry.FileErrorMessage;
                                    }
                                    else
                                    {
                                        workItem.LoadState = LibraryWorkItemState.CompleteSuccessful;
                                    }

                                    if (this.LibraryEntryLoaded != null)
                                        this.LibraryEntryLoaded(entry);
                                }
                                break;
                                case LibraryLoadType.M3UFile:
                                {
                                    var entry = LoadRadioEntry(workItem.FileName);

                                    // Set Work Item
                                    if (entry.FileError)
                                    {
                                        workItem.LoadState = LibraryWorkItemState.CompleteError;
                                        workItem.ErrorMessage = entry.FileErrorMessage;
                                    }
                                    else
                                    {
                                        workItem.LoadState = LibraryWorkItemState.CompleteSuccessful;
                                    }

                                    if (this.RadioEntryLoaded != null)
                                        this.RadioEntryLoaded(entry);
                                }
                                break;
                                default:
                                    throw new Exception("Worker load type not handled:  LibraryLoader.cs");
                            }
                        }
                    }

                    // Go ahead and report that we're done with the queue (needs event)
                    else if (_workQueue.Count == 0)
                    {
                        _userRun = false;
                    }
                }

                var currentState = GetState();       // ENTERS LOCK

                if (previousState != currentState)
                {
                    if (this.ProcessingChanged != null)
                        this.ProcessingChanged(currentState);
                }

                if (!workToProcess)
                {
                    Thread.Sleep(WORKER_SLEEP_PERIOD);
                }
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
