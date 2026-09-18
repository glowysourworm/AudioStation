using System.Collections.ObjectModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.Event;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    /// <summary>
    /// Sub-component of LibraryImporterViewModel
    /// </summary>
    public class LibraryImporterServiceWorkflowViewModel : ComponentPartViewModelBase
    {
        private IAudioStationController _audioStationController;
        private IAudioStationConfiguration _configuration;
        private IDialogController _dialogController;

        private readonly LibraryImporterConfigurationViewModel _workflowConfiguration;
        private readonly KeyedObservableCollection<string, LibraryImporterFileViewModel> _stagedFiles;

        private ObservableCollection<LibraryLoaderWorkerViewModelBase> _serviceWorkers;

        LibraryLoaderWorkerViewModelBase _selectedWorker;

        // ILibraryLoader State
        PlayStopPause _libraryLoaderState;

        SimpleCommand _moveToNextStepCommand;
        SimpleCommand _moveToPreviousStepCommand;
        SimpleCommand _rerunSelectedWorkItemsCommand;
        SimpleCommand _skipSelectedWorkItemsCommand;

        /// <summary>
        /// Event that occurs when a worker's work item is updated
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase, LibraryWorkItemViewModel> WorkItemChangedEvent;
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase, bool> StatusChangeEvent;

        public ObservableCollection<LibraryLoaderWorkerViewModelBase> ServiceWorkers
        {
            get { return _serviceWorkers; }
            set { this.RaiseAndSetIfChanged(ref _serviceWorkers, value); }
        }
        public LibraryLoaderWorkerViewModelBase SelectedWorker
        {
            get { return _selectedWorker; }
            set { this.RaiseAndSetIfChanged(ref _selectedWorker, value); }
        }
        public PlayStopPause LibraryLoaderState
        {
            get { return _libraryLoaderState; }
            set { this.RaiseAndSetIfChanged(ref _libraryLoaderState, value); }
        }
        public SimpleCommand MoveToNextStepCommand
        {
            get { return _moveToNextStepCommand; }
            set { this.RaiseAndSetIfChanged(ref _moveToNextStepCommand, value); }
        }
        public SimpleCommand MoveToPreviousStepCommand
        {
            get { return _moveToPreviousStepCommand; }
            set { this.RaiseAndSetIfChanged(ref _moveToPreviousStepCommand, value); }
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

        public LibraryImporterServiceWorkflowViewModel(LibraryImporterConfigurationViewModel configuration,
                                                       KeyedObservableCollection<string, LibraryImporterFileViewModel> stagedFiles) : base("Library Importer (loader)")
        {
            _workflowConfiguration = configuration;
            _stagedFiles = stagedFiles;

            // Might need to get this directly from the service for initialization
            this.LibraryLoaderState = PlayStopPause.Stop;

            this.MoveToNextStepCommand = new SimpleCommand(MoveToNextStep, CanMoveToNextStep);
            this.MoveToPreviousStepCommand = new SimpleCommand(MoveToPreviousStep, CanMoveToPreviousStep);
            this.RerunSelectedWorkItemsCommand = new SimpleCommand(RerunSelectedWorkItems, CanRerunSelectedWorkItems);
            this.SkipSelectedWorkItemsCommand = new SimpleCommand(SkipSelectedWorkItems, CanSkipSelectedWorkItems);

            this.SelectedWorker = null;
            this.ServiceWorkers = new ObservableCollection<LibraryLoaderWorkerViewModelBase>();
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

            this.SelectedWorker.ChangeState(loaderState);
        }
        public bool CanChangeLoaderState(PlayStopPause loaderState)
        {
            switch (loaderState)
            {
                case PlayStopPause.Play:
                    if (this.SelectedWorker != null &&
                       !this.SelectedWorker.Complete &&
                        this.LibraryLoaderState != PlayStopPause.Play)
                        return true;

                    break;
                case PlayStopPause.Pause:
                case PlayStopPause.Stop:
                    if (this.SelectedWorker != null &&
                        this.SelectedWorker.Working &&
                        this.LibraryLoaderState == PlayStopPause.Play)
                        return true;

                    break;
                default:
                    throw new Exception("Unhandled library loader state");
            }

            return false;
        }

        public override bool CanExecute()
        {
            return !this.Working &&
                    this.Loaded &&
                    this.SelectedWorker != null &&
                    this.SelectedWorker.CanExecute();
        }
        private bool CanLoadWorker()
        {
            return !this.Working &&
                    this.Loaded &&
                    this.LibraryLoaderState == PlayStopPause.Stop;
        }
        private bool CanMoveToPreviousStep()
        {
            var index = this.SelectedWorker == null ? -1 : this.ServiceWorkers.IndexOf(this.SelectedWorker);

            if (!this.Loaded)
                return false;

            if (index <= 0)
                return false;

            else
                return this.LibraryLoaderState == PlayStopPause.Stop;
        }
        private bool CanMoveToNextStep()
        {
            var index = this.SelectedWorker == null ? -1 : this.ServiceWorkers.IndexOf(this.SelectedWorker);

            if (!this.Loaded)
                return false;

            if (index == -1)
                return CanLoadWorker();

            else if (index == this.ServiceWorkers.Count - 1)
                return false;

            else
                return CanLoadWorker() && this.SelectedWorker != null && !this.SelectedWorker.Loaded;
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

        private void MoveToNextStep()
        {
            // TODO: Refactor this component "part" pattern
            _dialogController.ShowLoading("Loading Workflow Service", progressHandler =>
            {
                // Select Worker
                var index = this.SelectedWorker == null ? -1 : this.ServiceWorkers.IndexOf(this.SelectedWorker);

                if (index == -1)
                    this.SelectedWorker = this.ServiceWorkers.First();

                else
                    this.SelectedWorker = this.ServiceWorkers[index];

                // -> Load
                LoadPart(_configuration, _audioStationController, progressHandler);
            });
        }
        private void MoveToPreviousStep()
        {
            // Select Worker
            var index = this.SelectedWorker == null ? -1 : this.ServiceWorkers.IndexOf(this.SelectedWorker);

            if (index == this.ServiceWorkers.Count - 1)
                this.SelectedWorker = this.ServiceWorkers.Last();

            else if (index > 0)
                this.SelectedWorker = this.ServiceWorkers[index - 1];
        }
        private void RerunSelectedWorkItems()
        {
            this.SelectedWorker.RerunSelected();
        }
        private void SkipSelectedWorkItems()
        {
            this.SelectedWorker.SkipSelected();
        }

        protected void LoadPart(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.SelectedWorker == null)
                throw new ArgumentException("Must first select worker before loading");

            // Initialize Component Parts
            this.SelectedWorker.Load(configuration, audioStationController, progressHandler);
        }

        protected override void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Loader State Changes
            _configuration = configuration;
            _audioStationController = audioStationController;
            _dialogController = audioStationController.DialogController;

            var libraryLoaderService = audioStationController.ServiceController.GetService<ILibraryLoaderService>();
            var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();

            // Create Service Workers
            if (_workflowConfiguration.ServiceIncludeAcoustID)
                this.ServiceWorkers.Add(new LibraryLoaderAcoustIDViewModel(_workflowConfiguration, _stagedFiles));

            if (_workflowConfiguration.ServiceIncludeMusicBrainzBasic)
                this.ServiceWorkers.Add(new LibraryLoaderMusicBrainzBasicViewModel(_workflowConfiguration));

            if (_workflowConfiguration.ServiceIncludeMusicBrainzArtwork)
                this.ServiceWorkers.Add(new LibraryLoaderMusicBrainzAlbumArtViewModel(_workflowConfiguration));

            foreach (var worker in this.ServiceWorkers)
            {
                worker.StatusChangeEvent += OnWorkerStatusChangeEvent;
                worker.WorkItemChangedEvent += OnWorkerItemChangedEvent;
                worker.WorkItemUIChangedEvent += OnWorkerItemChangedEvent;
            }
        }

        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.SelectedWorker.Execute();
        }

        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.Working)
                throw new Exception("Cannot reset service workflow while it is running");

            // Call to unload some memory before completing workflow step
            foreach (var worker in this.ServiceWorkers)
                worker.Reset();
        }
        private void OnWorkerStatusChangeEvent(LibraryLoaderWorkerViewModelBase sender)
        {
            this.Working = this.ServiceWorkers.Any(x => x.Working);
            this.LibraryLoaderState = sender.LibraryLoaderState;

            UpdateCommands();

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(sender, this.Working);
        }
        private void OnWorkerItemChangedEvent(LibraryLoaderWorkerViewModelBase worker, LibraryWorkItemViewModel workItem)
        {
            UpdateCommands();

            if (this.WorkItemChangedEvent != null)
                this.WorkItemChangedEvent(worker, workItem);
        }
        private void UpdateCommands()
        {
            // Constructor sets properties
            if (this.MoveToNextStepCommand != null &&
                this.MoveToPreviousStepCommand != null &&
                this.RerunSelectedWorkItemsCommand != null &&
                this.SkipSelectedWorkItemsCommand != null)
            {
                this.MoveToNextStepCommand.RaiseCanExecuteChanged();
                this.MoveToPreviousStepCommand.RaiseCanExecuteChanged();
                this.RerunSelectedWorkItemsCommand.RaiseCanExecuteChanged();
                this.SkipSelectedWorkItemsCommand.RaiseCanExecuteChanged();
            }
        }
    }
}
