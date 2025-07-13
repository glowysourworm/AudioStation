using System.ComponentModel.DataAnnotations;

using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public enum LibraryLoadType
    {
        /// <summary>
        /// Loads mp3 file waiting for import. The tag data must meet minimum requirements for import (see IModelValidationService). AcoustID / Music Brainz are
        /// used as a means of getting data automatically, if needed.
        /// </summary>
        [Display(Name = "Load Mp3 File Into Library", Description = "Task that imports an mp3 file based on its tag data. This data may be input manually; or by using the AcoustID fingerprinting service along with Music Brainz tag data.")]
        ImportBasic,

        /// <summary>
        /// Loads mp3 file waiting for import. The tag data must (also) meet minimum requirements for import (see IModelValidationService). 
        /// The complete record of detailed data is filled out using Music Brainz; and any other album or fan art for the work.
        /// </summary>
        [Display(Name = "Load Mp3 File Into Library (Detail)", Description = "Task that imports an mp3 file based on its tag data; and also data collected using the Music Brainz service; and other artwork for the work.")]
        ImportDetail,

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
            _loadType = LibraryLoadType.ImportBasic;
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
