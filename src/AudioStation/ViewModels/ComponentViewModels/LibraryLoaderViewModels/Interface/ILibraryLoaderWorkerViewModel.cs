using AudioStation.Core.Component;

using SimpleWpf.Extensions.Event;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Interface
{
    public interface ILibraryLoaderWorkerViewModel
    {
        /// <summary>
        /// Executes when the library loader worker has changed status
        /// </summary>
        event SimpleEventHandler<ILibraryLoaderWorkerViewModel> StatusChangeEvent;

        /// <summary>
        /// Executes when work item is updated
        /// </summary>
        event SimpleEventHandler<ILibraryLoaderWorkerViewModel, LibraryWorkItemViewModel> WorkItemChangedEvent;

        /// <summary>
        /// Event that fires when any of the UI properties of the work item are changed (e.g. IsSelected)
        /// </summary>
        event SimpleEventHandler<ILibraryLoaderWorkerViewModel, LibraryWorkItemViewModel> WorkItemUIChangedEvent;

        bool Complete { get; }
        bool Working { get; }
        bool Loaded { get; }
        PlayStopPause LibraryLoaderState { get; }

        bool CanExecute();
        bool CanRerunSelected();
        bool CanSkipSelected();

        void ChangeState(PlayStopPause loaderState);

        void RerunSelected();
        void SkipSelected();

        void Execute();
        void Reset();
    }
}
