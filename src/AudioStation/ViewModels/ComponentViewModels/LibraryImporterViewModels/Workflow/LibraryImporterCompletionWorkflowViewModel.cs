using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterCompletionWorkflowViewModel : ComponentPartViewModelBase
    {
        private readonly IIocEventAggregator _eventAggregator;
        private readonly IAudioStationMapper _audioStationMapper;

        // Import Worker:  This will require a load of type ILibraryLoaderImportLoad. It operates on
        //                 the current workflow entities; and performs the rest of the import and file
        //                 handling tasks that are needed to complete the import workflow.
        //
        LibraryLoaderImportViewModel _importWorker;

        // Workflow
        LibraryImporterWorkflowViewModel _workflow;

        // Staged Files
        IEnumerable<LibraryImporterFileViewModel> _stagedFiles;

        public LibraryLoaderImportViewModel ImportWorker
        {
            get { return _importWorker; }
            set { this.RaiseAndSetIfChanged(ref _importWorker, value); }
        }
        public LibraryImporterWorkflowViewModel Workflow
        {
            get { return _workflow; }
            set { this.RaiseAndSetIfChanged(ref _workflow, value); }
        }
        public IEnumerable<LibraryImporterFileViewModel> StagedFiles
        {
            get { return _stagedFiles; }
            set { this.RaiseAndSetIfChanged(ref _stagedFiles, value); }
        }

        public LibraryImporterCompletionWorkflowViewModel(
                IIocEventAggregator eventAggregator,
                IAudioStationMapper audioStationMapper)
            : base("Library Importer (completion)")
        {
            _audioStationMapper = audioStationMapper;
            _eventAggregator = eventAggregator;
        }

        public void Execute()
        {
            if (this.ImportWorker.CanExecute())
                this.ImportWorker.Execute();
        }

        public bool CanExecute()
        {
            return !this.Loading;       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }

        protected override void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();

            this.ImportWorker = new LibraryLoaderImportViewModel(_eventAggregator,
                                                                 _audioStationMapper,
                                                                 libraryLoaderWorkerService,
                                                                 this.Workflow.Configuration,
                                                                 this.StagedFiles);

            this.ImportWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;

            // Initialize Component Parts
            this.ImportWorker.Initialize(configuration, audioStationController, progressHandler);
        }

        protected override void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Sub-components (DUPLICATE LOAD!) (NEEDS DESIGN)
            this.ImportWorker.Load(configuration, audioStationController, progressHandler);
        }

        private void OnWorkerStatusChangeEvent(LibraryLoaderWorkerViewModelBase sender)
        {
            this.Loading = !this.ImportWorker.IsWorkComplete;
        }
    }
}
