using AudioStation.Core.Component;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderWorkerService : IAudioStationService
    {
        /// <summary>
        /// Work item that is initialized by another component. The load type will designate how to 
        /// handle the load / output types. The method will return an ID from the ILibraryLoader on
        /// the back end. This should be stored as your ID in the work item and kept to reference
        /// during updates.
        /// </summary>
        int RunLoaderTaskAsync(LibraryWorkItemViewModel workItem);

        /// <summary>
        /// Attempts to set the state of the loader. Events are forwarded to respond.
        /// </summary>
        void ChangeLoaderState(PlayStopPause state);

        /// <summary>
        /// Returns true if the work item is queued
        /// </summary>
        bool IsTaskQueued(int workItemId);

        /// <summary>
        /// Returns true if the work item is running. Running work items may be cancelled.
        /// </summary>
        bool IsTaskRunning(int workItemId);

        /// <summary>
        /// Removes work item from the loader worker service
        /// </summary>
        void DequeueTask(int workItemId);

        /// <summary>
        /// Cancels task (if thread is running it will stop the thread); and removes from the queue.
        /// </summary>
        void CancelTask(int workItemId);
    }
}
