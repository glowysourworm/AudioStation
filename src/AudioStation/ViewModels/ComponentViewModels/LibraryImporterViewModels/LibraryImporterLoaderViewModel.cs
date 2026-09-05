using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.EventHandler;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels
{
    /// <summary>
    /// Sub-component of LibraryImporterViewModel
    /// </summary>
    public class LibraryImporterLoaderViewModel : ComponentViewModelBase
    {
        private LibraryImporterConfigurationViewModel _importOptions;


        LibraryLoaderAcoustIDViewModel _acoustIDWorker;
        LibraryLoaderMusicBrainzBasicViewModel _musicBrainzBasicWorker;
        LibraryLoaderMusicBrainzAlbumArtViewModel _musicBrainzAlbumArtWorker;

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

        public LibraryImporterLoaderViewModel(LibraryImporterConfigurationViewModel importOptions)
        {
            _importOptions = importOptions;
        }

        public void Execute()
        {
            // Workflow 1:  AcoustID (loading?, is-complete?)
            if (_importOptions.IdentifyUsingAcoustID && this.AcoustIDWorker.CanExecute())
            {
                this.Loading = true;
                this.AcoustIDWorker.Execute();
            }

            // Workflow 2:  Music Brainz Basic
            if (_importOptions.IdentifyUsingMusicBrainz && this.MusicBrainzBasicWorker.CanExecute())
            {
                this.Loading = true;
                this.MusicBrainzBasicWorker.Execute();
            }

            // Workflow 3:  Music Brainz Album Art
            if (_importOptions.IncludeMusicBrainzArtwork && this.MusicBrainzAlbumArtWorker.CanExecute())
            {
                this.Loading = true;
                this.MusicBrainzAlbumArtWorker.Execute();
            }
        }

        public bool CanExecute()
        {
            return !this.Loading;       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationViewModelController viewModelController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.AcoustIDWorker = viewModelController.GetComponent<LibraryLoaderAcoustIDViewModel>();
            this.MusicBrainzBasicWorker = viewModelController.GetComponent<LibraryLoaderMusicBrainzBasicViewModel>();
            this.MusicBrainzAlbumArtWorker = viewModelController.GetComponent<LibraryLoaderMusicBrainzAlbumArtViewModel>();
        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IComponentViewModelLoader viewModelLoader, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Sub-components (DUPLICATE LOAD!) (NEEDS DESIGN)
            this.AcoustIDWorker.Load(configuration, viewModelLoader, progressHandler);
            this.MusicBrainzBasicWorker.Load(configuration, viewModelLoader, progressHandler);
            this.MusicBrainzAlbumArtWorker.Load(configuration, viewModelLoader, progressHandler);
        }
    }
}
