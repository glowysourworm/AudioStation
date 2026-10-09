using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Model.Interface;

using SimpleWpf.Extensions.Event;

using static AudioStation.Event.DialogEventHandlers;
using static AudioStation.ViewModels.ServiceComponentDelegates;

namespace AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels.Interface
{
    public interface ILibraryLoaderWorkerViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Event that executes when the working status has changed
        /// </summary>
        public event ServiceComponentStatusUpdateHandler StatusChangeEvent;

        /// <summary>
        /// Executes when work item is updated
        /// </summary>
        event SimpleEventHandler<ILibraryLoaderWorkerViewModel, LibraryWorkItemViewModel> WorkItemChangedEvent;

        /// <summary>
        /// Event that fires when any of the UI properties of the work item are changed (e.g. IsSelected)
        /// </summary>
        event SimpleEventHandler<ILibraryLoaderWorkerViewModel, LibraryWorkItemViewModel> WorkItemUIChangedEvent;

        /// <summary>
        /// Executes when work item is updated
        /// </summary>
        event SimpleEventHandler<ILibraryLoaderWorkerViewModel, LibraryBulkWorkItemViewModel> BulkWorkItemChangedEvent;

        /// <summary>
        /// Event that fires when any of the UI properties of the work item are changed (e.g. IsSelected)
        /// </summary>
        event SimpleEventHandler<ILibraryLoaderWorkerViewModel, LibraryBulkWorkItemViewModel> BulkWorkItemUIChangedEvent;

        bool Complete { get; }
        bool Working { get; }
        bool Loaded { get; }
        bool ExecuteAsBulk { get; }
        PlayStopPause LibraryLoaderState { get; }

        /// <summary>
        /// The work item object is expected to be of the proper type implemented by the class. This may
        /// only be called while the worker is idle. Please use "CanAddWork" to check before adding.
        /// </summary>
        void AddWork(object workItem);

        /// <summary>
        /// The work item object is expected to be of the proper type implemented by the class. This may
        /// only be called while the worker is idle. Please use "CanAddWork" to check before adding.
        /// </summary>
        void AddWork(IEnumerable<object> workItem);

        /// <summary>
        /// Returns true if the worker can add work
        /// </summary>
        bool CanAddWork();

        bool CanExecute();
        bool CanLoad();
        bool CanReset();

        void Execute();
        void Load();
        void Reset();

        void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        void Execute(DialogProgressHandler progressHandler);
        void Reset(DialogProgressHandler progressHandler);

        bool CanRerunSelected();
        bool CanSkipSelected();

        void ChangeState(PlayStopPause loaderState);

        void RerunSelected();
        void SkipSelected();
    }
}
