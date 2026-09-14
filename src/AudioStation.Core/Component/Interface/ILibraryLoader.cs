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
        int RunLoaderTaskAsync(LibraryLoadType loadType, int workflowId, bool isWorkflowTask, object load);

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
    }
}
