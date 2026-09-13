using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterCompletionWorkflowViewModel : ComponentPartViewModelBase
    {
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

        public LibraryImporterCompletionWorkflowViewModel()
            : base("Library Importer (completion)")
        {
        }

        public override bool CanExecute()
        {
            return this.ImportWorker.CanExecute();       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }

        protected override void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var libraryLoaderWorkerService = audioStationController.ServiceController.GetService<ILibraryLoaderWorkerService>();

            this.ImportWorker = new LibraryLoaderImportViewModel(this.Workflow.Configuration, this.StagedFiles);

            this.ImportWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;

            // Initialize Component Parts
            this.ImportWorker.Load(configuration, audioStationController, progressHandler);
        }

        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker.Execute(progressHandler);
        }

        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker.Reset(progressHandler);
        }

        private void OnWorkerStatusChangeEvent(LibraryLoaderWorkerViewModelBase sender)
        {

        }
    }
}
