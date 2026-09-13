using AudioStation.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.UI.Command;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    /// <summary>
    /// Sub-component of LibraryImporterViewModel
    /// </summary>
    public class LibraryImporterServiceWorkflowViewModel : ComponentPartViewModelBase
    {
        LibraryLoaderAcoustIDViewModel _acoustIDWorker;
        LibraryLoaderMusicBrainzBasicViewModel _musicBrainzBasicWorker;
        LibraryLoaderMusicBrainzAlbumArtViewModel _musicBrainzAlbumArtWorker;

        // Workflow
        LibraryImporterWorkflowViewModel _workflow;

        SimpleCommand _executeCommand;

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

        public LibraryImporterServiceWorkflowViewModel(LibraryImporterWorkflowViewModel workflow) : base("Library Importer (loader)")
        {
            this.Workflow = workflow;

            this.AcoustIDWorker = new LibraryLoaderAcoustIDViewModel();
            this.MusicBrainzBasicWorker = new LibraryLoaderMusicBrainzBasicViewModel();
            this.MusicBrainzAlbumArtWorker = new LibraryLoaderMusicBrainzAlbumArtViewModel();
        }

        public override bool CanExecute()
        {
            return !this.Working;       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }

        protected override void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var libraryLoaderService = audioStationController.ServiceController.GetService<ILibraryLoaderService>();
            var libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();
            var audioStationDbClient = audioStationController.ServiceController.GetDataService<IAudioStationDbClient>();

            this.AcoustIDWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.MusicBrainzBasicWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;
            this.MusicBrainzAlbumArtWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;

            // Initialize Component Parts
            this.AcoustIDWorker.Load(configuration, audioStationController, progressHandler);
            this.MusicBrainzBasicWorker.Load(configuration, audioStationController, progressHandler);
            this.MusicBrainzAlbumArtWorker.Load(configuration, audioStationController, progressHandler);
        }
        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            if (!this.AcoustIDWorker.IsWorkComplete && this.Workflow.Configuration.ServiceIncludeAcoustID)
            {
                if (this.AcoustIDWorker.CanExecute())
                    this.AcoustIDWorker.Execute(progressHandler);
                else
                    throw new Exception("AcoustIDWorker not available for execution");
            }
            else if (!this.MusicBrainzBasicWorker.IsWorkComplete && this.Workflow.Configuration.ServiceIncludeMusicBrainzBasic)
            {
                if (this.MusicBrainzBasicWorker.CanExecute())
                    this.MusicBrainzBasicWorker.Execute(progressHandler);
                else
                    throw new Exception("MusicBrainzBasicWorker not available for execution");
            }
            else if (!this.MusicBrainzAlbumArtWorker.IsWorkComplete && this.Workflow.Configuration.ServiceIncludeMusicBrainzArtwork)
            {
                if (this.MusicBrainzAlbumArtWorker.CanExecute())
                    this.MusicBrainzAlbumArtWorker.Execute(progressHandler);
                else
                    throw new Exception("MusicBrainzAlbumArtWorker not available for execution");
            }
            else
            {
                // Complete
                ApplicationHelpers.Log("Import workflow execution complete! (Workflow={0})", this.Workflow.Name);

                // Unlock UI
                this.Working = false;
            }
        }

        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {

        }
        private void OnWorkerStatusChangeEvent(LibraryLoaderWorkerViewModelBase sender)
        {
            this.Working = this.AcoustIDWorker.Working ||
                           this.MusicBrainzBasicWorker.Working ||
                           this.MusicBrainzAlbumArtWorker.Working;
        }
    }
}
