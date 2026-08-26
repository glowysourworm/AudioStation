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
        /// Initializes and runs a library loader task with the specified parameters. Returns ID of new work item.
        /// </summary>
        int RunLoaderTaskAsync(LibraryLoadType loadType, object load);

        /// <summary>
        /// Queries the component to get a bulk report on workers left in the loader. This will only return true 
        /// if there are no threads currently running.
        /// </summary>
        bool IsWorkCompleted();
    }
}
