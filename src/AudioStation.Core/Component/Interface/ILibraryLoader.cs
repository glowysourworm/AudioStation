using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Component.Interface
{
    public interface ILibraryLoader : IDisposable
    {
        /// <summary>
        /// Sends updates from any work item (idle / in process)
        /// </summary>
        public event SimpleEventHandler<LibraryWorkItem> WorkItemUpdate;

        /// <summary>
        /// Sends an update to remind the UI dispatcher to check the ILibraryLoader for
        /// any work item updates.
        /// </summary>
        public event SimpleEventHandler ProcessingUpdate;

        PlayStopPause GetState();

        /// <summary>
        /// Gets a thread-safe copy of work items for an update from the loader core
        /// </summary>
        public IEnumerable<LibraryWorkItem> GetIdleWorkItems();

        /// <summary>
        /// Stops the work queue
        /// </summary>
        void Stop();

        /// <summary>
        /// Starts (or restarts) the work queue
        /// </summary>
        void Start();

        /// <summary>
        /// Clears the work queue - which will stop automatically when it is done. The
        /// worker thread will continue to run in the background, waiting for another
        /// re-start.
        /// </summary>
        void Clear();

        /// <summary>
        /// Initializes and runs a library loader task with the specified parameters.
        /// </summary>
        void RunLoaderTask(LibraryLoaderParameters parameters);
    }
}
