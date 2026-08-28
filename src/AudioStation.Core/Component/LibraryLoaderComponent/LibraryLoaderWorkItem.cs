using System.ComponentModel.DataAnnotations;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public enum LibraryLoadType
    {
        /// <summary>
        /// Loads mp3 file waiting for import. The tag data must (also) meet minimum requirements for import (see IModelValidationService). 
        /// The complete record of detailed data is filled out using Music Brainz; and any other album or fan art for the work.
        /// </summary>
        [Display(Name = "Load Mp3 File Into Library", Description = "Task that imports an mp3 file based on its tag data; and also data collected using the Music Brainz service; and other artwork for the work.")]
        Import,

        /// <summary>
        /// Opens m3u file, and adds it to the M3UStream table int the database.
        /// </summary>
        [Display(Name = "Load M3U Files Into Library", Description = "This task will load M3U's for internet streaming radio only. This is not to be used for Mp3 file management; and will not overwrite any existing radio entries.")]
        ImportRadio,

        /// <summary>
        /// Opens m3u file, and adds it to the M3UStream table int the database.
        /// </summary>
        [Display(Name = "AcoustID Download", Description = "This task will download results for the AcoustID audio fingerprint service and store them locally.")]
        AcoustID,

        /// <summary>
        /// Import small amount of tag data to local database using the AcoustID matching output.
        /// </summary>
        [Display(Name = "Music Brainz (basic)", Description = "This task will retrieve basic Music Brainz data for any completed AcoustID records in your library")]
        MusicBrainzBasic,

        /// <summary>
        /// Import album art from Music Brainz for tags loaded using the Music Brainz / AcoustID matching library loader service
        /// </summary>
        [Display(Name = "Music Brainz Album Art", Description = "This task will retrieve basic Music Brainz album artwork for all (basic) data retrieved using library loader services")]
        MusicBrainzAlbumArt,

        /// <summary>
        /// Checks integrity of FileReference table, and all related entities
        /// </summary>
        [Display(Name = "Music Brainz Album Art", Description = "This task will retrieve basic Music Brainz album artwork for all (basic) data retrieved using library loader services")]
        FileChecker
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
        LibraryLoaderLoad _workItem;                                           // Supposed to be a LibraryLoaderLoadBase
        LibraryLoaderOutput _outputItem;
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
        public LibraryLoaderLoad GetWorkItem()
        {
            lock (_lock)
            {
                return _workItem;
            }
        }
        public LibraryLoaderOutput GetOutputItem()
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
        public void Initialize(LibraryWorkItemState state, LibraryLoaderLoad workItem, LibraryLoaderOutput outputItem)
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
