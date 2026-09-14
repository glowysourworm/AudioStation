using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.Service.Interface;

using SimpleWpf.Extensions.Event;
using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public abstract class LibraryLoaderWorkerViewModelBase : ViewModelBase
    {
        private ILibraryLoaderWorkerService _libraryLoaderWorkerService;

        string _name;
        string _description;
        string _lastWorkItemMessage;
        int _workflowId;
        bool _isWorkflowTask;
        bool _isAllWorkComplete;
        bool _loaded;
        bool _working;

        ObservableCollection<LibraryWorkItemViewModel> _workItems;

        int _workItemsWaiting;
        int _workItemsInProgress;
        int _workItemsSuccessful;
        int _workItemsError;
        double _workProgress;


        /// <summary>
        /// Executes when the library loader worker has changed status
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase> StatusChangeEvent;

        /// <summary>
        /// Executes when work item is updated
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase, LibraryWorkItemViewModel> WorkItemChangedEvent;

        public string Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
        }
        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
        public string LastWorkItemMessage
        {
            get { return _lastWorkItemMessage; }
            set { this.RaiseAndSetIfChanged(ref _lastWorkItemMessage, value); }
        }
        public int WorkflowId
        {
            get { return _workflowId; }
            set { this.RaiseAndSetIfChanged(ref _workflowId, value); }
        }
        public bool Loaded
        {
            get { return _loaded; }
            set { this.RaiseAndSetIfChanged(ref _loaded, value); }
        }
        public bool Working
        {
            get { return _working; }
            set { this.RaiseAndSetIfChanged(ref _working, value); }
        }
        public bool IsWorkflowTask
        {
            get { return _isWorkflowTask; }
            set { this.RaiseAndSetIfChanged(ref _isWorkflowTask, value); }
        }
        public bool IsAllWorkComplete
        {
            get { return _isAllWorkComplete; }
            set { this.RaiseAndSetIfChanged(ref _isAllWorkComplete, value); }
        }
        public ObservableCollection<LibraryWorkItemViewModel> WorkItems
        {
            get { return _workItems; }
            set { this.RaiseAndSetIfChanged(ref _workItems, value); }
        }
        public int WorkItemsInProgress
        {
            get { return _workItemsInProgress; }
            set { this.RaiseAndSetIfChanged(ref _workItemsInProgress, value); }
        }
        public int WorkItemsWaiting
        {
            get { return _workItemsWaiting; }
            set { this.RaiseAndSetIfChanged(ref _workItemsWaiting, value); }
        }
        public int WorkItemsSuccessful
        {
            get { return _workItemsSuccessful; }
            set { this.RaiseAndSetIfChanged(ref _workItemsSuccessful, value); }
        }
        public int WorkItemsError
        {
            get { return _workItemsError; }
            set { this.RaiseAndSetIfChanged(ref _workItemsError, value); }
        }
        public double WorkProgress
        {
            get { return _workProgress; }
            set { this.RaiseAndSetIfChanged(ref _workProgress, value); }
        }
        public string Status
        {
            get
            {
                if (!this.Loaded)
                    return "Not Loaded";

                else if (this.Working)
                    return "Running";

                else if (this.WorkProgress >= 1)
                    return "Completed";

                else
                    return "Stopped";
            }
        }

        public LibraryLoaderWorkerViewModelBase(string name, string description, int workflowId, bool isWorkflowTask)
        {
            this.Name = name;
            this.Description = description;
            this.WorkItems = new ObservableCollection<LibraryWorkItemViewModel>();
            this.WorkflowId = workflowId;
            this.Working = false;
            this.IsWorkflowTask = isWorkflowTask;
        }

        public virtual void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Events
            audioStationController.EventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(OnWorkItemComplete);
            audioStationController.EventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Subscribe(OnWorkItemUpdate);

            _libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();

            OnUpdate();
        }
        public void Execute()
        {
            if (!CanExecute())
                throw new Exception("Loader task currently running. Please call 'CanExecute' first to verify it is finished.");

            foreach (var workItem in this.WorkItems.Where(x => !x.IsCompleted))
            {
                // WORK ITEM:  Id is set from the backend!
                workItem.Id = _libraryLoaderWorkerService.RunLoaderTaskAsync(workItem);
            }

            OnUpdate();
        }
        public void Reset()
        {
            this.WorkItems.Clear();

            OnUpdate();
        }

        /// <summary>
        /// Attempts a state change of the loader service. This will not affect the front end workflow items except
        /// for whatever occurs in the proceeding state.
        /// </summary>
        public void ChangeState(PlayStopPause loaderState)
        {
            _libraryLoaderWorkerService.ChangeLoaderState(loaderState);
        }

        public bool CanExecute()
        {
            // Reset must be done from loader component
            return this.Loaded &&
                  !this.Working &&
                   this.WorkItems.Count > 0 &&
                   this.WorkItems.Any(x => !x.IsCompleted && !x.InProgress);
        }

        protected override void OnPropertyChanged(string name)
        {
            if (name != "Status")
                base.OnPropertyChanged(name);

            else
            {
                base.OnPropertyChanged(name);

                if (this.StatusChangeEvent != null)
                    this.StatusChangeEvent(this);
            }
        }

        private void OnWorkItemUpdate(LibraryWorkItemViewModel model)
        {
            var workItem = this.WorkItems.FirstOrDefault(x => x.Id == model.Id);

            if (workItem != null)
            {
                Map(model, workItem);

                this.LastWorkItemMessage = model.LastMessage;

                if (this.WorkItemChangedEvent != null)
                    this.WorkItemChangedEvent(this, workItem);
            }

            OnUpdate();
        }
        private void OnWorkItemComplete(LibraryWorkItemViewModel model)
        {
            var workItem = this.WorkItems.FirstOrDefault(x => x.Id == model.Id);

            if (workItem != null)
            {
                Map(model, workItem);

                this.LastWorkItemMessage = model.LastMessage;

                if (this.WorkItemChangedEvent != null)
                    this.WorkItemChangedEvent(this, workItem);
            }

            OnUpdate();
        }

        /// <summary>
        /// Updates work item counter properties
        /// </summary>
        protected void OnUpdate()
        {
            if (this.Loaded)
            {
                this.WorkItemsWaiting = this.WorkItems.Count(x => !x.InProgress && !x.IsCompleted);
                this.WorkItemsInProgress = this.WorkItems.Count(x => x.InProgress);
                this.WorkItemsSuccessful = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && !x.HasErrors);
                this.WorkItemsError = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && x.HasErrors);
                this.WorkProgress = (this.WorkItemsSuccessful + this.WorkItemsError) / (double)this.WorkItems.Count;
                this.Working = this.WorkItems.Any(x => !x.IsCompleted && x.InProgress);
                this.IsAllWorkComplete = this.WorkItems.All(x => x.IsCompleted);

                OnPropertyChanged("Status");
            }
        }
        private void Map(LibraryWorkItemViewModel source, LibraryWorkItemViewModel dest)
        {
            if (source.Id != dest.Id)
                throw new ArgumentException("Trying to map mis-matching work items");

            dest.HasErrors = source.HasErrors;
            dest.InProgress = source.InProgress;
            dest.IsCompleted = source.IsCompleted;
            dest.LogMessages = source.LogMessages;
            //dest.Output = source.Output;
            dest.Progress = source.Progress;
            dest.WorkSteps = source.WorkSteps;
        }
    }
}
