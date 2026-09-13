using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
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
        int _workflowId;
        bool _loaded;
        bool _working;

        ObservableCollection<LibraryWorkItemViewModel> _workItems;

        int _workItemsWaiting;
        int _workItemsInProgress;
        int _workItemsSuccessful;
        int _workItemsError;
        double _workProgress;
        bool _isWorkComplete;
        bool _isWorkflowTask;

        /// <summary>
        /// Executes when the library loader worker has changed status
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase> StatusChangeEvent;

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
        public bool IsWorkComplete
        {
            get { return _isWorkComplete; }
            set { this.RaiseAndSetIfChanged(ref _isWorkComplete, value); }
        }
        public string Status
        {
            get
            {
                if (!this.Loaded)
                    return "Not Loaded";

                else if (this.Working)
                    return "Working";

                else if (this.IsWorkComplete)
                    return "Completed";

                else
                    return "Idle";
            }
        }

        public LibraryLoaderWorkerViewModelBase(string name, string description, int workflowId, bool isWorkflowTask)
        {
            this.Name = name;
            this.Description = description;
            this.WorkItems = new ObservableCollection<LibraryWorkItemViewModel>();
        }

        public virtual void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Events
            audioStationController.EventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(OnWorkItemComplete);
            audioStationController.EventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Subscribe(OnWorkItemUpdate);

            _libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();
        }
        public virtual void Execute(DialogEventHandlers.DialogProgressHandler progressHandler)
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
        public virtual void Reset(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.WorkItems.Clear();

            OnUpdate();
        }
        public bool CanExecute()
        {
            return this.Loaded && !this.Working && !this.IsWorkComplete;           // Reset must be done from loader component
        }

        protected override void OnPropertyChanged(string name)
        {
            if (name != "Status")
                base.OnPropertyChanged(name);

            else
                OnPropertyChanged("Status");

            OnUpdate();
        }

        private void OnWorkItemUpdate(LibraryWorkItemViewModel model)
        {
            var workItem = this.WorkItems.FirstOrDefault(x => x.Id == model.Id);

            if (workItem != null)
            {
                Map(model, workItem);
            }

            OnUpdate();
        }
        private void OnWorkItemComplete(LibraryWorkItemViewModel model)
        {
            var workItem = this.WorkItems.FirstOrDefault(x => x.Id == model.Id);

            if (workItem != null)
            {
                Map(model, workItem);
            }

            OnUpdate();
        }
        private void OnUpdate()
        {
            if (this.Loaded)
            {
                this.WorkItemsWaiting = this.WorkItems.Count(x => !x.InProgress && !x.IsCompleted);
                this.WorkItemsInProgress = this.WorkItems.Count(x => x.InProgress);
                this.WorkItemsSuccessful = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && !x.HasErrors);
                this.WorkItemsError = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && x.HasErrors);
                this.WorkProgress = (this.WorkItemsSuccessful + this.WorkItemsError) / (double)this.WorkItems.Count;
                this.IsWorkComplete = this.WorkItems.All(x => x.IsCompleted);

                if (this.StatusChangeEvent != null)
                    this.StatusChangeEvent(this);
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
