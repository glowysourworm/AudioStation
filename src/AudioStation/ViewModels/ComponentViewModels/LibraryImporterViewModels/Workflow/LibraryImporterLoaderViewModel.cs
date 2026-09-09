using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
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
    public class LibraryImporterLoaderViewModel : ComponentPartViewModelBase
    {
        private LibraryImporterConfigurationViewModel _importOptions;
        private readonly IAudioConverter _audioConverter;
        private readonly IIocEventAggregator _eventAggregator;

        LibraryLoaderAcoustIDViewModel _acoustIDWorker;
        LibraryLoaderMusicBrainzBasicViewModel _musicBrainzBasicWorker;
        LibraryLoaderMusicBrainzAlbumArtViewModel _musicBrainzAlbumArtWorker;
        LibraryLoaderFileConverterViewModel _fileConverterWorker;

        // Saved Workflow
        LibraryImporterWorkflowViewModel _selectedWorkflow;

        public LibraryImporterConfigurationViewModel ImportOptions
        {
            get { return _importOptions; }
            set { this.RaiseAndSetIfChanged(ref _importOptions, value); }
        }
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
        public LibraryLoaderFileConverterViewModel FileConverterWorker
        {
            get { return _fileConverterWorker; }
            set { this.RaiseAndSetIfChanged(ref _fileConverterWorker, value); }
        }
        public LibraryImporterWorkflowViewModel SelectedWorkflow
        {
            get { return _selectedWorkflow; }
            set { this.RaiseAndSetIfChanged(ref _selectedWorkflow, value); }
        }

        public LibraryImporterLoaderViewModel(IIocEventAggregator eventAggregator, IAudioConverter audioConverter, LibraryImporterConfigurationViewModel importOptions) : base("Library Importer (loader)")
        {
            _eventAggregator = eventAggregator;
            _audioConverter = audioConverter;

            this.ImportOptions = importOptions;
        }

        public void Execute()
        {
            if (this.SelectedWorkflow.ImproperShutdown)
                return;

            // Workflow 1:  AcoustID (loading?, is-complete?)
            if (_importOptions.IdentifyUsingAcoustID && this.AcoustIDWorker.CanExecute() && this.SelectedWorkflow.StepNumber <= 1)
            {
                this.Loading = true;
                this.AcoustIDWorker.Execute();
            }

            // Workflow 2:  Music Brainz Basic
            else if (_importOptions.IdentifyUsingMusicBrainz && this.MusicBrainzBasicWorker.CanExecute() && this.SelectedWorkflow.StepNumber == 2)
            {
                this.Loading = true;
                this.MusicBrainzBasicWorker.Execute();
            }

            // Workflow 3:  Music Brainz Album Art
            else if (_importOptions.IncludeMusicBrainzArtwork && this.MusicBrainzAlbumArtWorker.CanExecute() && this.SelectedWorkflow.StepNumber == 3)
            {
                this.Loading = true;
                this.MusicBrainzAlbumArtWorker.Execute();
            }

            // Workflow 4:  Convert Audio Files (post migration)
            else if (_importOptions.ConvertAudioFormat && this.FileConverterWorker.CanExecute() && this.SelectedWorkflow.StepNumber == 4)
            {
                this.Loading = true;
                this.FileConverterWorker.Execute();
            }
        }

        private void ExecuteWorkflowStep(int stepNumber)
        {
            switch (stepNumber)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                default:
                    break;
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
            this.FileConverterWorker = new LibraryLoaderFileConverterViewModel(_audioConverter, _eventAggregator, libraryLoaderWorkerService);

            this.AcoustIDWorker.StatusChangeEvent -= OnWorkerStatusChangeEvent;
            this.MusicBrainzBasicWorker.StatusChangeEvent -= OnWorkerStatusChangeEvent;
            this.MusicBrainzAlbumArtWorker.StatusChangeEvent -= OnWorkerStatusChangeEvent;
            this.FileConverterWorker.StatusChangeEvent -= OnWorkerStatusChangeEvent;

            this.AcoustIDWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.MusicBrainzBasicWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.MusicBrainzAlbumArtWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.FileConverterWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Sub-components (DUPLICATE LOAD!) (NEEDS DESIGN)
            this.AcoustIDWorker.Load(configuration, audioStationController, progressHandler);
            this.MusicBrainzBasicWorker.Load(configuration, audioStationController, progressHandler);
            this.MusicBrainzAlbumArtWorker.Load(configuration, audioStationController, progressHandler);
            this.FileConverterWorker.Load(configuration, audioStationController, progressHandler);
        }

        private void OnWorkerStatusChangeEvent(LibraryLoaderWorkerViewModelBase sender)
        {
            // Running Workflow Steps
            if (this.Loading)
            {
                if (this.AcoustIDWorker.Loading ||
                    this.MusicBrainzBasicWorker.Loading ||
                    this.MusicBrainzAlbumArtWorker.Loading ||
                    this.FileConverterWorker.Loading)
                    return;

                // Workers Complete
                else
                    this.Loading = false;
            }

            // Check for more work
            if (!this.Loading && this.SelectedWorkflow.StepNumber < 4)
            {
                // Increment Workflow Step


                // Execute Workflow Step
                ExecuteWorkflowStep(this.SelectedWorkflow.StepNumber);
            }
        }
    }
}
