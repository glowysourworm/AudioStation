using SimpleWpf.RecursiveSerializer.Shared;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public enum LibraryLoadType
    {
        /// <summary>
        /// Opens mp3 file, and adds it to the Mp3FileReference collections in the database. This 
        /// should be run on startup to update any file related information. 
        /// </summary>
        Mp3FileAddUpdate,

        /// <summary>
        /// Opens m3u file, and adds it to the M3UStream table int the database.
        /// </summary>
        M3UFileAddUpdate
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
        LibraryLoadType _loadType;
        LibraryWorkItemState _loadState;

        object _lock = new object();

        // Default constructor used for .Equals comparison / FirstOrDefault / etc...
        public LibraryLoaderWorkItem()
        {
            _id = -1;
            _startTime = DateTime.MinValue;
            _lastUpdateTime = DateTime.MinValue;
            _loadType = LibraryLoadType.Mp3FileAddUpdate;
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
        }

        public int GetId()
        {
            lock(_lock)
            {
                return _id;
            }            
        }
        public bool GetHasErrors()
        {
            lock ( _lock)
            {
                return _workItem.HasErrors();
            }
        }
        public DateTime GetStartTime()
        {
            lock(_lock)
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
            lock(_lock)
            {
                return _workItem.GetProgress();
            }
        }
        public void Initialize<T>(LibraryWorkItemState state, T workItem) where T : LibraryLoaderLoadBase
        {
            lock (_lock)
            {
                _loadState = state;
                _workItem = workItem;
            }
        }

        public void Start()
        {
            lock(_lock)
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
