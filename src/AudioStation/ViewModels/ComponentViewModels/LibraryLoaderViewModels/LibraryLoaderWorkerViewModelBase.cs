using System.Collections.ObjectModel;
using System.Windows.Threading;

using AudioStation.Core.Model.Interface;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;

using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public abstract class LibraryLoaderWorkerViewModelBase : ViewModelBase, IDisposable
    {
        private readonly ILibraryLoaderWorkerService _libraryLoaderService;

        ObservableCollection<LibraryWorkItemViewModel> _workItems;

        int _workItemsWaiting;
        int _workItemsInProgress;
        int _workItemsSuccessful;
        int _workItemsError;

        bool _loading;

        SimpleCommand _executeCommand;

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

        public bool Loading
        {
            get { return _loading; }
            set { this.RaiseAndSetIfChanged(ref _loading, value); }
        }

        public SimpleCommand ExecuteCommand
        {
            get { return _executeCommand; }
            set { this.RaiseAndSetIfChanged(ref _executeCommand, value); }
        }

        public LibraryLoaderWorkerViewModelBase(IIocEventAggregator eventAggregator, ILibraryLoaderWorkerService libraryLoaderService)
        {
            _libraryLoaderService = libraryLoaderService;

            this.WorkItems = new ObservableCollection<LibraryWorkItemViewModel>();

            this.ExecuteCommand = new SimpleCommand(Execute, CanExecute);
            this.Loading = false;

            eventAggregator.GetEvent<LibraryLoaderWorkItemCompleteEvent>().Subscribe(OnWorkItemComplete);
            eventAggregator.GetEvent<LibraryLoaderWorkItemUpdateEvent>().Subscribe(OnWorkItemUpdate);
        }

        private void Execute()
        {
            foreach (var workItem in this.WorkItems)
            {
                workItem.Progress = 0;

                // WORK ITEM:  Id is set from the backend!
                workItem.Id = _libraryLoaderService.RunLoaderTaskAsync(workItem);
            }

            OnUpdate();
        }
        private bool CanExecute()
        {
            return !this.Loading;
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
            if (this.ExecuteCommand != null)
            {
                this.ExecuteCommand.RaiseCanExecuteChanged();

                this.Loading = this.WorkItems.Any(x => x.InProgress);
                this.WorkItemsWaiting = this.WorkItems.Count(x => !x.InProgress && !x.IsCompleted);
                this.WorkItemsInProgress = this.WorkItems.Count(x => x.InProgress);
                this.WorkItemsSuccessful = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && !x.HasErrors);
                this.WorkItemsError = this.WorkItems.Count(x => !x.InProgress && x.IsCompleted && x.HasErrors);
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

        /// <summary>
        /// Function to initialize work items - will forward the request to the dispatcher thread
        /// </summary>
        /// <param name="configuration">Valid configuration</param>
        /// <param name="progressHandler">Progress callback</param>
        public void InitializeWorkItems(IAudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(InitializeWorkItems, DispatcherPriority.Background, configuration, progressHandler);

            else
            {
                InitializeWorkItemsRun(configuration, progressHandler);
                OnUpdate();
            }
        }

        /// <summary>
        /// Function to initialize the component (this is called from the Dispatcher)
        /// </summary>
        protected abstract void InitializeWorkItemsRun(IAudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler);

        public abstract void Dispose();
    }
}
