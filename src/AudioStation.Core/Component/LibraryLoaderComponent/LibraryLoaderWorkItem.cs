using System.ComponentModel.DataAnnotations;

using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public enum LibraryLoadType
    {
        /// <summary>
        /// Loads mp3 files waiting for import. These would be in either a staging directory or a download 
        /// directory. The import process will:  1) Get an AcoustID fingerprint, 2) Get MusicBrainz data for
        /// the track, 3) Insert all local records into the database, and 4) Import the file by moving it into
        /// the appropriate (templated) directory. This is configurable.
        /// </summary>
        [Display(Name = "Load Mp3 Files Into Library", Description = "Task that opens all mp3 files, and adds them to the Mp3FileReference collection in the database. This will not overwrite any existing records.")]
        Import,

        /// <summary>
        /// Opens m3u file, and adds it to the M3UStream table int the database.
        /// </summary>
        [Display(Name = "Load M3U Files Into Library", Description = "This task will load M3U's for internet streaming radio only. This is not to be used for Mp3 file management; and will not overwrite any existing radio entries.")]
        ImportRadio,

        /// <summary>
        /// Completely fill out, and report on music brainz ID's througout the library. The AcoustID
        /// service is used to try finding the ID by the first 30 seconds of the track in the event
        /// that we can't find it otherwise. Finally, the data is stored both in the database; and the
        /// tag data.
        /// </summary>
        [Display(Name = "Get Music Brainz Detail", Description = "This long running task will retrieve all applicable data from Music Brainz for your entire library; and store it locally in your database. This does not alter your existing tags - which can be done during other tasks.")]
        DownloadMusicBrainz
    }
    public enum LibraryWorkItemState
    {
        Pending = 0,
        Processing = 1,
        CompleteSuccessful = 2,
        CompleteError = 3
    }
    public class LibraryLoaderWorkItem
    {
        int _id;
        DateTime _startTime;
        DateTime _lastUpdateTime;
        LibraryLoaderLoadBase _workItem;                                           // Supposed to be a LibraryLoaderLoadBase
        LibraryLoaderOutputBase _outputItem;
        LibraryLoadType _loadType;
        LibraryWorkItemState _loadState;

        object _lock = new object();

        // Default constructor used for .Equals comparison / FirstOrDefault / etc...
        public LibraryLoaderWorkItem()
        {
            _id = -1;
            _startTime = DateTime.MinValue;
            _lastUpdateTime = DateTime.MinValue;
            _loadType = LibraryLoadType.Import;
            _loadState = LibraryWorkItemState.Pending;
        }
        public LibraryLoaderWorkItem(int id, LibraryLoadType loadType)
        {
            _id = id;
            _startTime = DateTime.MinValue;
            _lastUpdateTime = DateTime.MinValue;
            _loadType = loadType;
            _loadState = LibraryWorkItemState.Pending;
        }
        public LibraryLoaderWorkItem(LibraryLoaderWorkItem copy)
        {
            _id = copy.GetId();
            _startTime = copy.GetStartTime();
            _lastUpdateTime = copy.GetLastUpdateTime();
            _loadType = copy.GetLoadType();
            _loadState = copy.GetLoadState();
            _workItem = copy.GetWorkItem();
            _outputItem = copy.GetOutputItem();
        }

        public int GetId()
        {
            lock (_lock)
            {
                return _id;
            }
        }
        public int GetFailureCount()
        {
            lock (_lock)
            {
                return _workItem.GetFailureCount();
            }
        }
        public int GetSuccessCount()
        {
            lock (_lock)
            {
                return _workItem.GetSuccessCount();
            }
        }
        public bool GetHasErrors()
        {
            lock (_lock)
            {
                return _workItem.HasErrors();
            }
        }
        public DateTime GetStartTime()
        {
            lock (_lock)
            {
                return _startTime;
            }
        }
        public DateTime GetLastUpdateTime()
        {
            lock (_lock)
            {
                return _lastUpdateTime;
            }
        }
        public LibraryLoaderLoadBase GetWorkItem()
        {
            lock (_lock)
            {
                return _workItem;
            }
        }
        public LibraryLoaderOutputBase GetOutputItem()
        {
            lock (_lock)
            {
                return _outputItem;
            }
        }
        public LibraryLoadType GetLoadType()
        {
            lock (_lock)
            {
                return _loadType;
            }
        }
        public LibraryWorkItemState GetLoadState()
        {
            lock (_lock)
            {
                return _loadState;
            }
        }
        public double GetPercentComplete()
        {
            lock (_lock)
            {
                return _workItem.GetProgress();
            }
        }
        public void Initialize<TIn, TOut>(LibraryWorkItemState state, TIn workItem, TOut outputItem) where TIn : LibraryLoaderLoadBase
                                                                                                     where TOut : LibraryLoaderOutputBase
        {
            lock (_lock)
            {
                _loadState = state;
                _workItem = workItem;
                _outputItem = outputItem;
            }
        }

        public void Start()
        {
            lock (_lock)
            {
                _startTime = DateTime.Now;
                _loadState = LibraryWorkItemState.Processing;
            }
        }

        public void Update(LibraryWorkItemState state)
        {
            lock (_lock)
            {
                _loadState = state;
                _lastUpdateTime = DateTime.Now;
            }
        }
    }
}
