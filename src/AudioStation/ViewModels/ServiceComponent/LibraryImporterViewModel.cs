using System.Collections.ObjectModel;
using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.DataComponent.MainViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow;
using AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels.Worker;

using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.ViewModels.ServiceComponent
{
    public class LibraryImporterViewModel : ServiceComponentViewModelBase
    {
        // Configuration:  This is for the partial configuration editing control area for library directories.
        //
        AudioStationConfigurationViewModel _configuration;
        ObservableCollection<AudioEncoderViewModel> _encoders;

        // Workflow Configuration
        LibraryImporterConfigurationViewModel _workflowConfiguration;

        // Workflow Component Parts
        LibraryImporterServiceWorkflowViewModel _serviceWorkflow;
        LibraryImporterStagingWorkflowViewModel _stagingWorkflow;
        LibraryImporterCompletionWorkflowViewModel _completionWorkflow;
        LibraryImporterServiceWorkflowViewModel _importWorkflow;

        // Workflow Steps
        LibraryImporterWorkflowStep _workflowCurrentStep;
        bool _workflowNextEnabled;
        bool _workflowPreviousEnabled;

        // Readonly Convenience Properties (part of the configuration)
        string _sourceFolder;
        string _destinationFolder;

        int _sourceFileCount;
        int _sourceFileTodoCount;

        string _sourceFolderSearch;
        string _stagedSearch;

        SimpleCommand _cleanupImportCommand;

        public AudioStationConfigurationViewModel Configuration
        {
            get { return _configuration; }
            set { this.RaiseAndSetIfChanged(ref _configuration, value); }
        }
        public ObservableCollection<AudioEncoderViewModel> Encoders
        {
            get { return _encoders; }
            set { this.RaiseAndSetIfChanged(ref _encoders, value); }
        }
        public LibraryImporterConfigurationViewModel WorkflowConfiguration
        {
            get { return _workflowConfiguration; }
            set { this.RaiseAndSetIfChanged(ref _workflowConfiguration, value); }
        }
        public LibraryImporterServiceWorkflowViewModel ServiceWorkflow
        {
            get { return _serviceWorkflow; }
            set { this.RaiseAndSetIfChanged(ref _serviceWorkflow, value); }
        }
        public LibraryImporterStagingWorkflowViewModel StagingWorkflow
        {
            get { return _stagingWorkflow; }
            set { this.RaiseAndSetIfChanged(ref _stagingWorkflow, value); }
        }
        public LibraryImporterCompletionWorkflowViewModel CompletionWorkflow
        {
            get { return _completionWorkflow; }
            set { this.RaiseAndSetIfChanged(ref _completionWorkflow, value); }
        }
        public LibraryImporterServiceWorkflowViewModel ImportWorkflow
        {
            get { return _importWorkflow; }
            set { this.RaiseAndSetIfChanged(ref _importWorkflow, value); }
        }
        public LibraryImporterWorkflowStep WorkflowCurrentStep
        {
            get { return _workflowCurrentStep; }
            private set { this.RaiseAndSetIfChanged(ref _workflowCurrentStep, value); }
        }

        public bool WorkflowNextEnabled
        {
            get { return _workflowNextEnabled; }
            private set { this.RaiseAndSetIfChanged(ref _workflowNextEnabled, value); }
        }
        public bool WorkflowPreviousEnabled
        {
            get { return _workflowPreviousEnabled; }
            private set { this.RaiseAndSetIfChanged(ref _workflowPreviousEnabled, value); }
        }

        public int SourceFileCount
        {
            get { return _sourceFileCount; }
            set { this.RaiseAndSetIfChanged(ref _sourceFileCount, value); }
        }
        public int SourceFileTodoCount
        {
            get { return _sourceFileTodoCount; }
            set { this.RaiseAndSetIfChanged(ref _sourceFileTodoCount, value); }
        }
        public string SourceFolder
        {
            get { return _sourceFolder; }
            set { this.RaiseAndSetIfChanged(ref _sourceFolder, value); }
        }
        public string DestinationFolder
        {
            get { return _destinationFolder; }
            set { this.RaiseAndSetIfChanged(ref _destinationFolder, value); }
        }

        public string SourceFolderSearch
        {
            get { return _sourceFolderSearch; }
            set { this.RaiseAndSetIfChanged(ref _sourceFolderSearch, value); }
        }
        public string StagedSearch
        {
            get { return _stagedSearch; }
            set { this.RaiseAndSetIfChanged(ref _stagedSearch, value); }
        }

        public SimpleCommand CleanupImportCommand
        {
            get { return _cleanupImportCommand; }
            set { this.RaiseAndSetIfChanged(ref _cleanupImportCommand, value); }
        }

        public LibraryImporterViewModel(IAudioStationMapper audioStationMapper,
                                        IAudioConverter audioConverter,
                                        IDialogController dialogController,
                                        IIocEventAggregator eventAggregator,
                                        ITagCache tagCacheController) : base("Library Importer")
        {
            this.WorkflowConfiguration = new LibraryImporterConfigurationViewModel();
            this.StagingWorkflow = new LibraryImporterStagingWorkflowViewModel(dialogController, this.WorkflowConfiguration);
            this.ServiceWorkflow = new LibraryImporterServiceWorkflowViewModel(this.StagingWorkflow.StagedFiles);
            this.ImportWorkflow = new LibraryImporterServiceWorkflowViewModel(this.StagingWorkflow.StagedFiles);
            this.CompletionWorkflow = new LibraryImporterCompletionWorkflowViewModel(this.StagingWorkflow.StagedFiles, this.WorkflowConfiguration);

            // These are very light weight to bubble up the workflow property changes
            // 
            this.WorkflowConfiguration.PropertyChanged += OnBubbleUpViewModelEvent;
            this.StagingWorkflow.PropertyChanged += OnBubbleUpViewModelEvent;
            this.ServiceWorkflow.PropertyChanged += OnBubbleUpViewModelEvent;
            this.ImportWorkflow.PropertyChanged += OnBubbleUpViewModelEvent;
            this.CompletionWorkflow.PropertyChanged += OnBubbleUpViewModelEvent;

            // Component Parts:  Status Listeners + Event Forwarding
            //
            // When you add parts to the component - it will automatically follow the parts with
            // its own Loading and Loaded flags. So, you'll need to take that into account when
            // you expose your CanExecute,... functions.
            //
            this.AddComponentPart(this.StagingWorkflow);
            this.AddComponentPart(this.ServiceWorkflow);
            this.AddComponentPart(this.CompletionWorkflow);
            this.AddComponentPart(this.ImportWorkflow);

            // Workflow
            this.WorkflowCurrentStep = LibraryImporterWorkflowStep.Configuration;

            // Import Completion
            this.CleanupImportCommand = new SimpleCommand(CleanupImport, CanCleanupImport);

            Update();
        }

        protected override void OnStatusChanged()
        {
            // -> (currently nothing to do)
            base.OnStatusChanged();

            if (this.Initialized)
                Update();
        }

        public void WorkflowNext()
        {
            switch (_workflowCurrentStep)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.ConfigurationOptions;
                    break;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.Staging;
                    break;
                case LibraryImporterWorkflowStep.Staging:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.ServiceWorkers;
                    break;
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.TagCompletion;
                    break;
                case LibraryImporterWorkflowStep.TagCompletion:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.ImportCompletion;
                    break;
                case LibraryImporterWorkflowStep.ImportCompletion:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.FinalReport;
                    break;
                case LibraryImporterWorkflowStep.FinalReport:
                    break;
                default:
                    throw new Exception("Unhandled import step type");
            }

            PreLoadServices(_workflowCurrentStep);
            Update();
        }
        public void WorkflowPrevious()
        {
            switch (_workflowCurrentStep)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    break;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.Configuration;
                    break;
                case LibraryImporterWorkflowStep.Staging:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.ConfigurationOptions;
                    break;
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.Staging;
                    break;
                case LibraryImporterWorkflowStep.TagCompletion:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.ServiceWorkers;
                    break;
                case LibraryImporterWorkflowStep.ImportCompletion:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.TagCompletion;
                    break;
                case LibraryImporterWorkflowStep.FinalReport:
                    _workflowCurrentStep = LibraryImporterWorkflowStep.ImportCompletion;
                    break;
                default:
                    throw new Exception("Unhandled import step type");
            }

            Update();
        }

        /// <summary>
        /// Returns the component part associated with the workflow step. This would be
        /// the part that must be loaded prior to execution. This should allow you to 
        /// use the view model to manage the workflow. So, this does not depend on the
        /// current step of the workflow - just the one you are requesting.
        /// </summary>
        public ServiceComponentPartViewModelBase? GetWorkflowComponentPart(LibraryImporterWorkflowStep workflowStep)
        {
            switch (workflowStep)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    return null;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    return null;
                case LibraryImporterWorkflowStep.Staging:
                    return this.StagingWorkflow;
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    return this.ServiceWorkflow;
                case LibraryImporterWorkflowStep.TagCompletion:
                    return this.CompletionWorkflow;
                case LibraryImporterWorkflowStep.ImportCompletion:
                    return this.ImportWorkflow;
                case LibraryImporterWorkflowStep.FinalReport:
                    return null;
                default:
                    throw new Exception("Unhandled import step type");
            }
        }
        private void Update()
        {
            // Check Workflow Step Validation
            //
            switch (this.WorkflowCurrentStep)
            {
                // Validation:  Import Directory
                //
                case LibraryImporterWorkflowStep.Configuration:
                    this.WorkflowNextEnabled = !this.Loading && this.WorkflowConfiguration.ImportDirectory != null;
                    this.WorkflowPreviousEnabled = false;

                    // Source / Destination Folders
                    //
                    this.SourceFolder = this.WorkflowConfiguration.ImportType == LibraryImportType.InPlaceDirectory ?
                                        this.WorkflowConfiguration.ImportDirectory?.Directory ?? string.Empty :
                                        this.WorkflowConfiguration.MigrationSourceDirectory;

                    this.DestinationFolder = this.WorkflowConfiguration.ImportDirectory?.Directory ?? string.Empty;

                    break;

                // Validation: (warning) (recommended options)
                //
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    this.WorkflowNextEnabled = !this.Loading;
                    this.WorkflowPreviousEnabled = !this.Loading;
                    break;

                // Validation: Staged Files
                //
                case LibraryImporterWorkflowStep.Staging:
                    this.WorkflowNextEnabled = !this.Loading && this.StagingWorkflow.StagedFiles.TotalFileCount > 0;
                    this.WorkflowPreviousEnabled = !this.Loading;

                    // Execute Recursive File Count (probably not a performance issue; but check for too much UI interaction) (IsSelected Binding)
                    if (this.StagingWorkflow.ImportDirectory?.IsUpdating() ?? false)
                        this.SourceFileCount = this.StagingWorkflow.ImportDirectory.TotalFileCount;
                    break;

                // Validation: Service Workers (executed) (warnings?, errors?)
                //
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    this.WorkflowNextEnabled = !this.Loading;
                    this.WorkflowPreviousEnabled = !this.Loading;
                    break;

                // Validation: Staged Files (all have been imported or attempted) (warning?)
                //
                case LibraryImporterWorkflowStep.TagCompletion:
                    this.WorkflowNextEnabled = !this.Loading && this.CompletionWorkflow.StagedFiles.Any(x => x.ImportFile?.TagRecordDirty?.IsValid ?? false);
                    this.WorkflowPreviousEnabled = !this.Loading;
                    break;

                // Validation: TODO
                //
                case LibraryImporterWorkflowStep.ImportCompletion:
                    this.WorkflowNextEnabled = !this.Loading;
                    this.WorkflowPreviousEnabled = !this.Loading;
                    this.SourceFileTodoCount = this.SourceFileCount - this.StagingWorkflow.StagedFiles.Count(x => x.ImportFile?.ImportOutput?.ImportResult == LibraryWorkerResultLevel.Success);
                    break;

                // Validation: TODO
                //
                case LibraryImporterWorkflowStep.FinalReport:
                    this.WorkflowNextEnabled = false;
                    this.WorkflowPreviousEnabled = !this.Loading;
                    break;
                default:
                    throw new Exception("Unhandled workflow step");
            }
        }

        private void CleanupImport()
        {
            // Cleanup Datasets
        }
        private bool CanCleanupImport()
        {
            return !this.Loading &&
                    this.Loaded &&
                    this.WorkflowCurrentStep == LibraryImporterWorkflowStep.FinalReport;
        }

        public override bool CanExecute()
        {
            return this.GetWorkflowComponentPart(this.WorkflowCurrentStep)?.CanExecute() ?? false;
        }
        public override bool CanReset()
        {
            return this.GetWorkflowComponentPart(this.WorkflowCurrentStep)?.CanReset() ?? false;
        }
        public override bool CanLoad()
        {
            return this.GetWorkflowComponentPart(this.WorkflowCurrentStep)?.CanLoad() ?? false;
        }

        public override void Initialize(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            // Sub-component(s)
            this.Configuration = audioStationController.ComponentController.GetDataComponent<AudioStationConfigurationViewModel>();
            this.Encoders = audioStationController.ComponentController.GetDataComponent<MainViewModel>().Encoders;

            this.Initialized = true;
        }
        public override void Load(Guid? componentPartId, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Component Part
            if (componentPartId != null)
            {
                this.ComponentParts
                    .First(x => x.Id == componentPartId)
                    .Load(configuration, audioStationController, progressHandler);
            }
        }
        public override void Execute(Guid? componentPartId, DialogProgressHandler progressHandler)
        {
            // Component Part
            if (componentPartId != null)
            {
                this.ComponentParts
                    .First(x => x.Id == componentPartId)
                    .Execute(progressHandler);
            }
        }
        public override void Reset(Guid? componentPartId, DialogProgressHandler progressHandler)
        {
            // Component Part
            if (componentPartId != null)
            {
                this.ComponentParts
                    .First(x => x.Id == componentPartId)
                    .Reset(progressHandler);
            }

            // All Components
            else
            {
                // 1) Free up memory; 2) Set Loaded = false
                //
                foreach (var componentPart in this.ComponentParts)
                {
                    componentPart.Reset(progressHandler);
                }
            }
        }

        private void PreLoadServices(LibraryImporterWorkflowStep workflowStep)
        {
            switch (workflowStep)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    break;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    break;
                case LibraryImporterWorkflowStep.Staging:
                    break;
                case LibraryImporterWorkflowStep.ServiceWorkers:
                {
                    // Service Workflow - AcoustID
                    if (this.WorkflowConfiguration.ServiceIncludeAcoustID &&
                       !this.ServiceWorkflow.HasWorker<LibraryLoaderAcoustIDViewModel>())
                    {
                        this.ServiceWorkflow.AddWorker(new LibraryLoaderAcoustIDViewModel(this.WorkflowConfiguration, true));
                    }

                    // Service Workflow - Audio Duration
                    if (this.WorkflowConfiguration.ServiceIncludeAudioDuration &&
                       !this.ServiceWorkflow.HasWorker<LibraryLoaderAudioDurationViewModel>())
                    {
                        this.ServiceWorkflow.AddWorker(new LibraryLoaderAudioDurationViewModel(true));
                    }

                    // Service Workflow - Music Brainz (basic)
                    if (this.WorkflowConfiguration.ServiceIncludeMusicBrainzBasic &&
                       !this.ServiceWorkflow.HasWorker<LibraryLoaderMusicBrainzBasicViewModel>())
                    {
                        this.ServiceWorkflow.AddWorker(new LibraryLoaderMusicBrainzBasicViewModel(this.WorkflowConfiguration, true));
                    }

                    // Service Workflow - Music Brainz (artwork)
                    if (this.WorkflowConfiguration.ServiceIncludeMusicBrainzArtwork &&
                       !this.ServiceWorkflow.HasWorker<LibraryLoaderMusicBrainzAlbumArtViewModel>())
                    {
                        this.ServiceWorkflow.AddWorker(new LibraryLoaderMusicBrainzAlbumArtViewModel(this.WorkflowConfiguration, true));
                    }

                    // -> Select Worker
                    if (this.ServiceWorkflow.CanMoveNext())
                        this.ServiceWorkflow.MoveNext();
                }
                break;
                case LibraryImporterWorkflowStep.TagCompletion:
                    break;
                case LibraryImporterWorkflowStep.ImportCompletion:
                {
                    // Import Workflow (workers)
                    if (!this.ImportWorkflow.HasWorker<LibraryLoaderImportViewModel>())
                    {
                        this.ImportWorkflow.AddWorker(new LibraryLoaderImportViewModel(true));
                    }

                    // -> Select Worker
                    if (this.ImportWorkflow.CanMoveNext())
                        this.ImportWorkflow.MoveNext();
                }
                break;
                case LibraryImporterWorkflowStep.FinalReport:
                    break;
                default:
                    throw new Exception("Unhandled workflow step");
            }
        }

        private void EditTag()
        {
            //var inputFiles = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryImporterFileViewModel>().ToList();
            //var firstFile = inputFiles.FirstOrDefault();

            //if (firstFile == null)
            //    return;

            //// Base the tag view model on the first input. Then, build a group tag
            //// from there.
            ////
            //try
            //{
            //    // Get the current working tag
            //    var tag = firstFile.GetTagCopy();

            //    // Map tag to view model
            //    var viewModel = _audioStationMapper.Map<IAudioStationTag, TagViewModel>(tag);

            //    // Show Tag Editor (ONLY UPDATES NEW VIEW-MODEL! WE MUST MAP THE RESULT BACK!)
            //    var dialogResult = _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Editor (" + firstFile.ShortPath + ")", DialogEditorView.TagView, viewModel));

            //    // User wishes to save the data
            //    if (dialogResult)
            //    {
            //        // Update Import File (view model)(still in new memory only)
            //        firstFile.SaveTagEdit(viewModel);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ApplicationHelpers.Log("Application error:  {0}", LogLevel.Error, ex, ex.Message);
            //    throw ex;
            //}
        }
        private void EditTagGroup(string fieldName)
        {
            //var inputFiles = this.SourceDirectory.RecursiveWhere(x => !x.IsDirectory && x.IsSelected).Cast<LibraryImporterFileViewModel>().ToList();
            //var firstFile = inputFiles.FirstOrDefault();

            //if (firstFile == null)
            //    return;

            //// Base the tag view model on the first input. Then, build a group tag
            //// from there.
            ////
            //try
            //{
            //    // Create view model for editing
            //    var viewModel = new DialogTagFieldEditorViewModel()
            //    {
            //        TagFieldName = fieldName
            //    };

            //    // Show Tag Editor (ONLY UPDATES NEW VIEW-MODEL! WE MUST MAP THE RESULT BACK!)
            //    var dialogResult = _dialogController.ShowDialogWindowSync(DialogEventData.ShowDialogEditor("Tag Editor (Group)", DialogEditorView.TagFieldView, viewModel));

            //    // User wishes to save the data
            //    if (dialogResult)
            //    {
            //        foreach (var file in inputFiles)
            //        {
            //            // Update Import File:  Group Fields Only (view model) (still in new memory only)
            //            file.SaveTagFieldEdit(fieldName, viewModel.Tag);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    ApplicationHelpers.Log("Application error:  {0}", LogLevel.Error, ex, ex.Message);
            //    throw ex;
            //}
        }

        private void OnBubbleUpViewModelEvent(object? sender, PropertyChangedEventArgs e)
        {
            Update();
        }
    }
}
