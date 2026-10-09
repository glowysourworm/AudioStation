using System.Collections.ObjectModel;
using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels.Interface;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Event;
using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow
{
    /// <summary>
    /// Sub-component of LibraryImporterViewModel
    /// </summary>
    public class LibraryImporterServiceWorkflowViewModel : ServiceComponentPartViewModelBase
    {
        private readonly LibraryImporterFileTreeViewModel _stagedFiles;

        private ObservableCollection<ILibraryLoaderWorkerViewModel> _serviceWorkers;

        ILibraryLoaderWorkerViewModel _selectedWorker;

        // ILibraryLoader State
        PlayStopPause _libraryLoaderState;

        SimpleCommand _rerunSelectedWorkItemsCommand;
        SimpleCommand _skipSelectedWorkItemsCommand;

        /// <summary>
        /// Event that occurs when a worker's work item is updated
        /// </summary>
        public event SimpleEventHandler<ILibraryLoaderWorkerViewModel, LibraryWorkItemViewModel> WorkItemChangedEvent;

        public ObservableCollection<ILibraryLoaderWorkerViewModel> ServiceWorkers
        {
            get { return _serviceWorkers; }
            private set { this.RaiseAndSetIfChanged(ref _serviceWorkers, value); }
        }
        public ILibraryLoaderWorkerViewModel SelectedWorker
        {
            get { return _selectedWorker; }
            set { this.RaiseAndSetIfChanged(ref _selectedWorker, value); }
        }
        public PlayStopPause LibraryLoaderState
        {
            get { return _libraryLoaderState; }
            set { this.RaiseAndSetIfChanged(ref _libraryLoaderState, value); }
        }
        public SimpleCommand RerunSelectedWorkItemsCommand
        {
            get { return _rerunSelectedWorkItemsCommand; }
            set { this.RaiseAndSetIfChanged(ref _rerunSelectedWorkItemsCommand, value); }
        }
        public SimpleCommand SkipSelectedWorkItemsCommand
        {
            get { return _skipSelectedWorkItemsCommand; }
            set { this.RaiseAndSetIfChanged(ref _skipSelectedWorkItemsCommand, value); }
        }

        public LibraryImporterServiceWorkflowViewModel(LibraryImporterFileTreeViewModel stagedFiles)
            : base("Library Service(s)", "This workflow component will execute your selected services")
        {
            _stagedFiles = stagedFiles;

            // Might need to get this directly from the service for initialization
            this.LibraryLoaderState = PlayStopPause.Stop;

            this.RerunSelectedWorkItemsCommand = new SimpleCommand(RerunSelectedWorkItems, CanRerunSelectedWorkItems);
            this.SkipSelectedWorkItemsCommand = new SimpleCommand(SkipSelectedWorkItems, CanSkipSelectedWorkItems);

            this.SelectedWorker = null;

            this.ServiceWorkers = new ObservableCollection<ILibraryLoaderWorkerViewModel>();
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            UpdateCommands();
        }

        public void ChangeLoaderState(PlayStopPause loaderState)
        {
            if (!CanChangeLoaderState(loaderState))
                throw new Exception("Cannot change loader state - please check before trying to change");

            this.SelectedWorker.ChangeLoaderState(loaderState);
        }
        public bool CanChangeLoaderState(PlayStopPause loaderState)
        {
            return this.SelectedWorker != null && this.SelectedWorker.CanChangeLoaderState(loaderState);
        }

        public override bool CanExecute()
        {
            return !this.Working &&
                    this.Loaded &&
                    this.ServiceWorkers.Any() &&
                    this.SelectedWorker != null &&
                    this.SelectedWorker.CanExecute();
        }
        public override bool CanLoad()
        {
            return !this.Working &&
                   !this.Loaded &&
                    this.SelectedWorker != null &&
                    this.SelectedWorker.CanLoad() &&
                    this.SelectedWorker.CanAddWork();
        }
        public override bool CanReset()
        {
            return !this.Working &&
                    this.Loaded;
        }
        public bool CanMoveNext()
        {
            return !this.Working &&
                    this.ServiceWorkers.Any();
        }

        /// <summary>
        /// Moves selected worker to next in the list. Returns false if the selected worker is at
        /// the end of the list.
        /// </summary>
        public bool MoveNext()
        {
            if (!CanMoveNext())
                throw new Exception("Service workers list is empty. Please first add service workers before executing.");

            // Next
            var nextWorker = this.ServiceWorkers.Next(this.SelectedWorker);

            // End of List
            if (nextWorker == this.SelectedWorker)
                return false;

            // Select Worker
            this.SelectedWorker = nextWorker;

            return true;
        }
        private bool CanRerunSelectedWorkItems()
        {
            return this.SelectedWorker != null &&
                   this.SelectedWorker.CanRerunSelected();
        }
        private bool CanSkipSelectedWorkItems()
        {
            return this.SelectedWorker != null &&
                   this.SelectedWorker.CanSkipSelected();
        }
        private void RerunSelectedWorkItems()
        {
            this.SelectedWorker.RerunSelected();
        }
        private void SkipSelectedWorkItems()
        {
            this.SelectedWorker.SkipSelected();
        }

        public void AddWorker(ILibraryLoaderWorkerViewModel worker)
        {
            worker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            worker.WorkItemChangedEvent += OnWorkerItemChangedEvent;
            worker.WorkItemUIChangedEvent += OnWorkerItemChangedEvent;
            worker.PropertyChanged += OnWorkerPropertyChanged;

            _serviceWorkers.Add(worker);
        }

        public bool HasWorker<T>() where T : ILibraryLoaderWorkerViewModel
        {
            return _serviceWorkers.Any(x => x is T);
        }

        public override void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.SelectedWorker == null)
                throw new ArgumentException("Must first select worker before loading");

            if (!this.SelectedWorker.CanAddWork())
                throw new Exception("Selected worker cannot add work at this time. Please check state of the worker first.");

            if (!this.SelectedWorker.CanLoad())
                throw new Exception("Selected worker cannot load at this time. Please check state of the worker first.");

            // Load Workers:  This issue still has an overall casting problem we're avoiding. To templatize this whole
            //                LibraryLoader + Workflow design would take a couple more refactorings.
            //

            this.SelectedWorker.AddWork(_stagedFiles.Where(x => !x.IsDirectory));

            // -> Component Part Load
            this.SelectedWorker.Load(configuration, audioStationController, progressHandler);

            this.Loaded = true;
        }

        public override void Execute(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.SelectedWorker.Execute(progressHandler);
        }

        public override void Reset(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Call to unload some memory before completing workflow step
            foreach (var worker in this.ServiceWorkers)
                worker.Reset(progressHandler);
        }
        private void OnWorkerStatusChangeEvent(ServiceComponentPartViewModelBase sender, bool working, bool loaded)
        {
            this.Working = this.ServiceWorkers.Any(x => x.Working);
            this.LibraryLoaderState = (sender as ILibraryLoaderWorkerViewModel).LibraryLoaderState;

            UpdateCommands();
        }
        private void OnWorkerItemChangedEvent(ILibraryLoaderWorkerViewModel worker, LibraryWorkItemViewModel workItem)
        {
            UpdateCommands();

            if (this.WorkItemChangedEvent != null)
                this.WorkItemChangedEvent(worker, workItem);
        }
        private void OnWorkerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateCommands();
        }
        private void UpdateCommands()
        {
            // LibraryLoaderState (each worker is a listener!)
            if (this.SelectedWorker != null)
                this.LibraryLoaderState = this.SelectedWorker.LibraryLoaderState;

            // Constructor sets properties
            if (this.RerunSelectedWorkItemsCommand != null &&
                this.SkipSelectedWorkItemsCommand != null)
            {
                this.RerunSelectedWorkItemsCommand.RaiseCanExecuteChanged();
                this.SkipSelectedWorkItemsCommand.RaiseCanExecuteChanged();
            }
        }

        public override void Dispose()
        {
            // Nothing to do
        }
    }
}
