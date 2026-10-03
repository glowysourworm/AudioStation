namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    /// <summary>
    /// Bulk Work Item: This may be accessed from both dispatcher and work threads!
    /// </summary>
    public class LibraryLoaderBulkWorkItem
    {
        int _id;
        Guid _ownerId;

        Queue<LibraryLoaderWorkItem> _workItemsPending;
        Dictionary<int, LibraryLoaderWorkItem> _workItemsProcessing;
        Dictionary<int, LibraryLoaderWorkItem> _workItemsCompleted;

        LibraryLoadType _loadType;
        LibraryWorkItemState _state;

        object _lock;

        public int GetId()
        {
            lock (_lock)
            {
                return _id;
            }
        }
        public Guid GetOwnerId()
        {
            lock (_lock)
            {
                return _ownerId;
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
                return _state;
            }
        }
        public double GetProgress()
        {
            lock (_lock)
            {
                var totalPendingOrProcessing = _workItemsPending.Count + _workItemsProcessing.Count;
                return totalPendingOrProcessing / (double)(totalPendingOrProcessing + _workItemsCompleted.Count);
            }
        }
        public int GetCount()
        {
            lock (_lock)
            {
                return _workItemsCompleted.Count +
                       _workItemsPending.Count +
                       _workItemsProcessing.Count;
            }
        }
        public int GetCompletedCount()
        {
            lock (_lock)
            {
                return _workItemsCompleted.Count;
            }
        }
        public int GetCount(LibraryWorkItemState state)
        {
            lock (_lock)
            {
                var count = _workItemsPending.Count(x => x.GetLoadState() == state) +
                            _workItemsCompleted.Values.Count(x => x.GetLoadState() == state) +
                            _workItemsProcessing.Values.Count(x => x.GetLoadState() == state);

                return count;
            }
        }
        public int GetCount(LibraryWorkerResultLevel resultLevel)
        {
            lock (_lock)
            {
                var count = _workItemsPending.Where(x => x.GetOutputItem().ResultSteps.Any(x => x.Result == resultLevel)).Count() +
                            _workItemsCompleted.Values.Where(x => x.GetOutputItem().ResultSteps.Any(x => x.Result == resultLevel)).Count() +
                            _workItemsProcessing.Values.Where(x => x.GetOutputItem().ResultSteps.Any(x => x.Result == resultLevel)).Count();

                return count;
            }
        }

        /// <summary>
        /// Returns a reference to next work item for processing
        /// </summary>
        public LibraryLoaderWorkItem Dequeue()
        {
            lock (_lock)
            {
                if (_workItemsPending.Count == 0)
                    throw new Exception("No work items pending for processing");

                // Dequeue from pending
                var workItem = _workItemsPending.Dequeue();

                // Add to processing
                _workItemsProcessing.Add(workItem.GetId(), workItem);

                return workItem;
            }
        }

        public void ReportComplete(LibraryLoaderWorkItem workItem)
        {
            lock (_lock)
            {
                if (!_workItemsProcessing.ContainsKey(workItem.GetId()))
                    throw new Exception("Work item reference not found");

                // Remove from processing
                _workItemsProcessing.Remove(workItem.GetId());

                // Add to completed
                _workItemsCompleted.Add(workItem.GetId(), workItem);
            }
        }

        public void Start()
        {
            lock (_lock)
            {
                _state = LibraryWorkItemState.Processing;
            }
        }
        public void Update(LibraryWorkItemState state)
        {
            lock (_lock)
            {
                _state = state;
            }
        }

        public LibraryLoaderBulkWorkItem(int id, Guid ownerId, IEnumerable<LibraryLoaderWorkItem> workItems)
        {
            if (!workItems.Any())
                throw new ArgumentException("Library loader bulk item must contain work");

            _lock = new object();

            _id = id;
            _ownerId = ownerId;
            _state = LibraryWorkItemState.Pending;
            _loadType = workItems.First().GetLoadType();

            _workItemsPending = new Queue<LibraryLoaderWorkItem>(workItems);
            _workItemsProcessing = new Dictionary<int, LibraryLoaderWorkItem>();
            _workItemsCompleted = new Dictionary<int, LibraryLoaderWorkItem>();
        }
    }
}
