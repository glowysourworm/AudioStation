using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.LibraryLoaderEvent;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.UI.Event;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    /// <summary>
    /// Sub-component of LibraryImporterViewModel
    /// </summary>
    public class LibraryImporterServiceWorkflowViewModel : ComponentPartViewModelBase
    {
        private ILibraryLoaderWorkerService _libraryLoaderWorkerService;

        private readonly LibraryImporterConfigurationViewModel _workflowConfiguration;

        LibraryLoaderAcoustIDViewModel _acoustIDWorker;
        LibraryLoaderMusicBrainzBasicViewModel _musicBrainzBasicWorker;
        LibraryLoaderMusicBrainzAlbumArtViewModel _musicBrainzAlbumArtWorker;

        // ILibraryLoader State
        PlayStopPause _libraryLoaderState;

        /// <summary>
        /// Event that occurs when a worker's work item is updated
        /// </summary>
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase, LibraryWorkItemViewModel> WorkItemChangedEvent;
        public event SimpleEventHandler<LibraryLoaderWorkerViewModelBase, bool> StatusChangeEvent;

        public LibraryLoaderAcoustIDViewModel AcoustIDWorker
        {
            get { return _acoustIDWorker; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDWorker, value); }
        }
        public LibraryLoaderMusicBrainzBasicViewModel MusicBrainzBasicWorker
        {
            get { return _musicBrainzBasicWorker; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzBasicWorker, value); }
        }
        public LibraryLoaderMusicBrainzAlbumArtViewModel MusicBrainzAlbumArtWorker
        {
            get { return _musicBrainzAlbumArtWorker; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzAlbumArtWorker, value); }
        }
        public PlayStopPause LibraryLoaderState
        {
            get { return _libraryLoaderState; }
            set { this.RaiseAndSetIfChanged(ref _libraryLoaderState, value); }
        }

        public LibraryImporterServiceWorkflowViewModel(LibraryImporterConfigurationViewModel configuration) : base("Library Importer (loader)")
        {
            _workflowConfiguration = configuration;

            this.AcoustIDWorker = new LibraryLoaderAcoustIDViewModel(configuration);
            this.MusicBrainzBasicWorker = new LibraryLoaderMusicBrainzBasicViewModel(configuration);
            this.MusicBrainzAlbumArtWorker = new LibraryLoaderMusicBrainzAlbumArtViewModel(configuration);
        }

        public void ChangeLoaderState(PlayStopPause loaderState)
        {
            _libraryLoaderWorkerService.ChangeLoaderState(loaderState);
        }

        public override bool CanExecute()
        {
            return !this.Working && this.Loaded &&
                   (this.AcoustIDWorker.CanExecute() ||
                    this.MusicBrainzBasicWorker.CanExecute() ||
                    this.MusicBrainzAlbumArtWorker.CanExecute());       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }

        protected override void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Loader State Changes
            _libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();

            // -> On Loader State Change
            audioStationController.EventAggregator.GetEvent<LibraryLoaderStateChangeEvent>().Subscribe(OnLibraryLoaderStateChange);

            var libraryLoaderService = audioStationController.ServiceController.GetService<ILibraryLoaderService>();
            var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();

            this.AcoustIDWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.MusicBrainzBasicWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.MusicBrainzAlbumArtWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;

            this.AcoustIDWorker.WorkItemChangedEvent += OnWorkerItemChangedEvent;
            this.MusicBrainzBasicWorker.WorkItemChangedEvent += OnWorkerItemChangedEvent;
            this.MusicBrainzAlbumArtWorker.WorkItemChangedEvent += OnWorkerItemChangedEvent;

            // Initialize Component Parts
            this.AcoustIDWorker.Load(configuration, audioStationController, progressHandler);
            this.MusicBrainzBasicWorker.Load(configuration, audioStationController, progressHandler);
            this.MusicBrainzAlbumArtWorker.Load(configuration, audioStationController, progressHandler);
        }

        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.AcoustIDWorker.CanExecute() && _workflowConfiguration.ServiceIncludeAcoustID)
            {
                this.AcoustIDWorker.Execute();
            }
            else if (this.MusicBrainzBasicWorker.CanExecute() && _workflowConfiguration.ServiceIncludeMusicBrainzBasic)
            {
                this.MusicBrainzBasicWorker.Execute();
            }
            else if (this.MusicBrainzAlbumArtWorker.CanExecute() && _workflowConfiguration.ServiceIncludeMusicBrainzArtwork)
            {
                this.MusicBrainzAlbumArtWorker.Execute();
            }
            else
            {
                // Complete
                ApplicationHelpers.Log("Import workflow execution complete!");

                // Unlock UI
                this.Working = false;
            }
        }

        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (this.Working)
                throw new Exception("Cannot reset service workflow while it is running");

            this.AcoustIDWorker.Reset();
            this.MusicBrainzBasicWorker.Reset();
            this.MusicBrainzAlbumArtWorker.Reset();
        }
        private void OnWorkerStatusChangeEvent(LibraryLoaderWorkerViewModelBase sender)
        {
            this.Working = this.AcoustIDWorker.Working ||
                           this.MusicBrainzBasicWorker.Working ||
                           this.MusicBrainzAlbumArtWorker.Working;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(sender, this.Working);

            //// -> Next Task
            //if (CanExecute())
            //{
            //    this.Execute((x, y, z, w) => { });
            //}
        }
        private void OnWorkerItemChangedEvent(LibraryLoaderWorkerViewModelBase worker, LibraryWorkItemViewModel workItem)
        {
            if (this.WorkItemChangedEvent != null)
                this.WorkItemChangedEvent(worker, workItem);
        }
        private void OnLibraryLoaderStateChange(PlayStopPause libraryLoaderState)
        {
            this.LibraryLoaderState = libraryLoaderState;
        }
    }
}
