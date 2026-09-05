using System.Collections.ObjectModel;

using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.Service.Interface;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public abstract class LibraryLoaderWorkerViewModelBase : ComponentViewModelBase
    {
        private readonly ILibraryLoaderWorkerService _libraryLoaderService;

        string _name;
        string _description;

        ObservableCollection<LibraryWorkItemViewModel> _workItems;

        int _workItemsWaiting;
        int _workItemsInProgress;
        int _workItemsSuccessful;
        int _workItemsError;
        double _workProgress;
        bool _isWorkComplete;

        SimpleCommand _executeCommand;

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

        public SimpleCommand ExecuteCommand
        {
            get { return _executeCommand; }
            set { this.RaiseAndSetIfChanged(ref _executeCommand, value); }
        }

        public LibraryLoaderWorkerViewModelBase(string name, string description, IIocEventAggregator eventAggregator, ILibraryLoaderWorkerService libraryLoaderService)
            : base(name)
        {
            _libraryLoaderService = libraryLoaderService;

            this.Name = name;
            this.Description = description;
            this.WorkItems = new ObservableCollection<LibraryWorkItemViewModel>();

            this.ExecuteCommand = new SimpleCommand(Execute, CanExecute);
            this.Loading = false;

            eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(OnWorkItemComplete);
            eventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Subscribe(OnWorkItemUpdate);
        }

        public void Execute()
        {
            if (!CanExecute())
                throw new Exception("Loader task currently running. Please call 'CanExecute' first to verify it is finished.");

            foreach (var workItem in this.WorkItems)
            {
                workItem.Progress = 0;

                // WORK ITEM:  Id is set from the backend!
                workItem.Id = _libraryLoaderService.RunLoaderTaskAsync(workItem);
            }

            OnUpdate();
        }
        public bool CanExecute()
        {
            return this.Initialized && !this.Loading && !this.IsWorkComplete;           // Reset must be done from loader component
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

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
            if (this.Initialized)
            {
                this.Loading = this.WorkItems.Any(x => x.InProgress);
                this.WorkItemsWaiting = this.WorkItems.Count(x => !x.InProgress && !x.IsCompleted);
                this.WorkItemsInProgress = this.WorkItems.Count(x => x.InProgress);
                this.WorkItemsSuccessful = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && !x.HasErrors);
                this.WorkItemsError = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && x.HasErrors);
                this.WorkProgress = (this.WorkItemsSuccessful + this.WorkItemsError) / (double)this.WorkItems.Count;
                this.IsWorkComplete = this.WorkItems.All(x => x.IsCompleted);
            }

            if (this.ExecuteCommand != null)
                this.ExecuteCommand.RaiseCanExecuteChanged();
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
