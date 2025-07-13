using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad;
using AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderOutput;

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
        public event SimpleEventHandler<LibraryLoaderOutputBase> WorkItemComplete;

        /// <summary>
        /// Initializes and runs a library loader task with the specified parameters.
        /// </summary>
        void RunLoaderTaskAsync<TIn>(LibraryLoaderParameters<TIn> parameters) where TIn : LibraryLoaderLoadBase;

        /// <summary>
        /// Queries the component to get a bulk report on workers left in the loader. This will only return true 
        /// if there are no threads currently running.
        /// </summary>
        bool IsWorkCompleted();
    }
}
