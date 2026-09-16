using AudioStation.Core.Component.LibraryLoaderComponent;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.Interface
{
    public interface ILibraryLoader : IDisposable
    {
        /// <summary>
        /// Sends updates for a work item. These occur between work item processing steps.
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkItemUpdate> WorkItemUpdate;

        /// <summary>
        /// Sends completed event for a work item
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkItem> WorkItemComplete;

        /// <summary>
        /// Sends an event when the library loader changes state
        /// </summary>
        public event SimpleEventHandler<PlayStopPause> StateChangeEvent;

        /// <summary>
        /// Initializes and runs a library loader task with the specified parameters. Returns ID of new work item.
        /// </summary>
        int QueueLoaderTask(LibraryLoadType loadType, object load);

        /// <summary>
        /// Sets state of loader:  This will not alter any work items. It will only stop the loader from processing
        /// new ones (pause); Dequeue all worker tasks and send reports (stop); or resume any (pause) stopped work.
        /// </summary>
        void ChangeState(PlayStopPause state);

        /// <summary>
        /// Queries the component to get a bulk report on workers left in the loader. This will only return true 
        /// if there are no threads currently running.
        /// </summary>
        bool IsWorkCompleted();

        /// <summary>
        /// Returns true if the work item is queued
        /// </summary>
        bool IsTaskQueued(int workItemId);

        /// <summary>
        /// Returns true if the work item is running. Running work items may be cancelled.
        /// </summary>
        bool IsTaskRunning(int workItemId);

        /// <summary>
        /// Removes task from work queue. Task must not yet be running. All interaction with
        /// the queue is from the main thread.
        /// </summary>
        void DequeueTask(int workItemId);

        /// <summary>
        /// Cancels task (if thread is running it will stop the thread); and removes from the queue.
        /// </summary>
        void CancelTask(int workItemId);
    }
}
