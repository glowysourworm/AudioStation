using System.Collections.ObjectModel;

using AudioStation.Core;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using SimpleWpf.Extensions.Command;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels
{
    public abstract class LibraryLoaderComponentViewModelBase : ComponentViewModelBase<NoViewModel>
    {
        private readonly ILibraryLoaderService _libraryLoaderService;

        ObservableCollection<LibraryWorkItemViewModel> _workItems;

        int _workItemsWaiting;
        int _workItemsInProgress;
        int _workItemsSuccessful;
        int _workItemsError;

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

        public SimpleCommand ExecuteCommand
        {
            get { return _executeCommand; }
            set { this.RaiseAndSetIfChanged(ref _executeCommand, value); }
        }

        public override NoViewModel? Load { get; }

        public LibraryLoaderComponentViewModelBase(IIocEventAggregator eventAggregator, ILibraryLoaderService libraryLoaderService)
        {
            _libraryLoaderService = libraryLoaderService;

            this.WorkItems = new ObservableCollection<LibraryWorkItemViewModel>();

            this.ExecuteCommand = new SimpleCommand(Execute, CanExecute);

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
            dest.Output = source.Output;
            dest.Progress = source.Progress;
            dest.WorkSteps = source.WorkSteps;
        }

        /// <summary>
        /// Function to initialize the component (this is called from the Dispatcher)
        /// </summary>
        protected abstract void InitializeComponent(Configuration configuration, DialogEventHandlers.DialogProgressHandler progressHandler);

        public override void Initialize(Configuration configuration, NoViewModel load, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.BeginInvokeDispatcher(Initialize, System.Windows.Threading.DispatcherPriority.Background, configuration, load, progressHandler);

            else
            {
                InitializeComponent(configuration, progressHandler);
                OnUpdate();
            }
        }

        public override void Dispose()
        {
            // Nothing to do
        }
    }
}
