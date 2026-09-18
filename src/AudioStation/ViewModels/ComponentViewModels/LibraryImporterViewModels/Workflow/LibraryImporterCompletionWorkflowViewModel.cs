using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Workflow
{
    public class LibraryImporterCompletionWorkflowViewModel : ComponentPartViewModelBase
    {
        // Workflow Configuration
        private readonly LibraryImporterConfigurationViewModel _workflowConfiguration;

        // Staged Files
        private readonly KeyedObservableCollection<string, LibraryImporterFileViewModel> _stagedFiles;

        // Import Worker:  This will require a load of type ILibraryLoaderImportLoad. It operates on
        //                 the current workflow entities; and performs the rest of the import and file
        //                 handling tasks that are needed to complete the import workflow.
        //
        LibraryLoaderImportViewModel _importWorker;

        public LibraryLoaderImportViewModel ImportWorker
        {
            get { return _importWorker; }
            set { this.RaiseAndSetIfChanged(ref _importWorker, value); }
        }
        public IEnumerable<LibraryImporterFileViewModel> StagedFiles
        {
            get { return _stagedFiles; }
        }

        public LibraryImporterCompletionWorkflowViewModel(
                KeyedObservableCollection<string, LibraryImporterFileViewModel> stagedFiles,
                LibraryImporterConfigurationViewModel workflowConfiguration)
            : base("Library Importer (completion)")
        {
            _workflowConfiguration = workflowConfiguration;
            _stagedFiles = stagedFiles;
        }

        public override bool CanExecute()
        {
            return this.ImportWorker.CanExecute();       // We'll use this locally (besides LoadImpl) and set it during the workflow
        }

        protected override void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker = new LibraryLoaderImportViewModel(_workflowConfiguration, _stagedFiles);

            this.ImportWorker.StatusChangeEvent += OnWorkerStatusChangeEvent;

            // Initialize Component Parts
            this.ImportWorker.Load(configuration, audioStationController, progressHandler);
        }

        protected override void ExecuteWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker.Execute();
        }

        protected override void ResetWork(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            this.ImportWorker.Reset();
        }

        private void OnWorkerStatusChangeEvent(LibraryLoaderWorkerViewModelBase sender)
        {

        }
    }
}
