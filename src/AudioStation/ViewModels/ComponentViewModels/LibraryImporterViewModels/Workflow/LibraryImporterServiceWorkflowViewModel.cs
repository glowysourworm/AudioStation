using AudioStation.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    /// <summary>
    /// Sub-component of LibraryImporterViewModel
    /// </summary>
    public class LibraryImporterServiceWorkflowViewModel : ComponentPartViewModelBase
    {
        private readonly IIocEventAggregator _eventAggregator;

        LibraryLoaderAcoustIDViewModel _acoustIDWorker;
        LibraryLoaderMusicBrainzBasicViewModel _musicBrainzBasicWorker;
        LibraryLoaderMusicBrainzAlbumArtViewModel _musicBrainzAlbumArtWorker;

        // Workflow
        LibraryImporterWorkflowViewModel _workflow;

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
        public LibraryImporterWorkflowViewModel Workflow
        {
            get { return _workflow; }
            set { this.RaiseAndSetIfChanged(ref _workflow, value); }
        }

        public LibraryImporterServiceWorkflowViewModel(IIocEventAggregator eventAggregator, LibraryImporterWorkflowViewModel workflow) : base("Library Importer (loader)")
        {
            _eventAggregator = eventAggregator;

            this.Workflow = workflow;
        }

        public void Execute()
        {
            ExecuteNextWorkflowStep();
        }

        private void ExecuteNextWorkflowStep()
        {
            if (!this.AcoustIDWorker.IsWorkComplete && this.Workflow.Configuration.ServiceIncludeAcoustID)
            {
                if (this.AcoustIDWorker.CanExecute())
                    this.AcoustIDWorker.Execute();
                else
                    throw new Exception("AcoustIDWorker not available for execution");
            }
            else if (!this.MusicBrainzBasicWorker.IsWorkComplete && this.Workflow.Configuration.ServiceIncludeMusicBrainzBasic)
            {
                if (this.MusicBrainzBasicWorker.CanExecute())
                    this.MusicBrainzBasicWorker.Execute();
                else
                    throw new Exception("MusicBrainzBasicWorker not available for execution");
            }
            else if (!this.MusicBrainzAlbumArtWorker.IsWorkComplete && this.Workflow.Configuration.ServiceIncludeMusicBrainzArtwork)
            {
                if (this.MusicBrainzAlbumArtWorker.CanExecute())
                    this.MusicBrainzAlbumArtWorker.Execute();
                else
                    throw new Exception("MusicBrainzAlbumArtWorker not available for execution");
            }
            else
            {
                // Complete
                ApplicationHelpers.Log("Import workflow execution complete! (Workflow={0})", this.Workflow.Name);

                // Unlock UI
                this.Loading = false;
            }
        }

        public bool CanExecute()
        {
            return !this.Loading;       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var libraryLoaderService = audioStationController.ServiceController.GetService<ILibraryLoaderService>();
            var libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();
            var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();

            this.AcoustIDWorker = new LibraryLoaderAcoustIDViewModel(_eventAggregator, libraryLoaderWorkerService);
            this.MusicBrainzBasicWorker = new LibraryLoaderMusicBrainzBasicViewModel(_eventAggregator, libraryLoaderWorkerService, audioStationDbClient);
            this.MusicBrainzAlbumArtWorker = new LibraryLoaderMusicBrainzAlbumArtViewModel(_eventAggregator, libraryLoaderWorkerService, audioStationDbClient);

            this.AcoustIDWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.MusicBrainzBasicWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.MusicBrainzAlbumArtWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;

            // Initialize Component Parts
            this.AcoustIDWorker.Initialize(configuration, audioStationController, progressHandler);
            this.MusicBrainzBasicWorker.Initialize(configuration, audioStationController, progressHandler);
            this.MusicBrainzAlbumArtWorker.Initialize(configuration, audioStationController, progressHandler);
        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Sub-components (DUPLICATE LOAD!) (NEEDS DESIGN)
            this.AcoustIDWorker.Load(configuration, audioStationController, progressHandler);
            this.MusicBrainzBasicWorker.Load(configuration, audioStationController, progressHandler);
            this.MusicBrainzAlbumArtWorker.Load(configuration, audioStationController, progressHandler);
        }

        private void OnWorkerStatusChangeEvent(LibraryLoaderWorkerViewModelBase sender)
        {
            // Running Workflow Steps
            if (this.Loading)
            {
                if (this.AcoustIDWorker.Loading ||
                    this.MusicBrainzBasicWorker.Loading ||
                    this.MusicBrainzAlbumArtWorker.Loading)
                    return;

                // Workers Complete (check for more work)
                else
                    ExecuteNextWorkflowStep();
            }
        }
    }
}
