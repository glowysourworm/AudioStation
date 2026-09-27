using System.Collections.ObjectModel;
using System.ComponentModel;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Event;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.DataComponent.MainViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow;

using SimpleWpf.IocFramework.EventAggregation;

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

        // Workflow Steps
        LibraryImporterWorkflowStep _workflowCurrentStep;
        bool _workflowNextEnabled;
        bool _workflowPreviousEnabled;

        string _sourceFolderSearch;
        string _stagedSearch;

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

        public LibraryImporterViewModel(IAudioStationMapper audioStationMapper,
                                        IAudioConverter audioConverter,
                                        IDialogController dialogController,
                                        IIocEventAggregator eventAggregator,
                                        ITagCache tagCacheController) : base("Library Importer")
        {
            this.WorkflowConfiguration = new LibraryImporterConfigurationViewModel();
            this.StagingWorkflow = new LibraryImporterStagingWorkflowViewModel(dialogController, this.WorkflowConfiguration);
            this.ServiceWorkflow = new LibraryImporterServiceWorkflowViewModel(this.WorkflowConfiguration, this.StagingWorkflow.StagedFiles);
            this.CompletionWorkflow = new LibraryImporterCompletionWorkflowViewModel(this.StagingWorkflow.StagedFiles, this.WorkflowConfiguration);

            // These are very light weight to bubble up the workflow property changes
            // 
            this.WorkflowConfiguration.PropertyChanged += OnBubbleUpViewModelEvent;
            this.StagingWorkflow.PropertyChanged += OnBubbleUpViewModelEvent;
            this.ServiceWorkflow.PropertyChanged += OnBubbleUpViewModelEvent;
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

            // Workflow
            this.WorkflowCurrentStep = LibraryImporterWorkflowStep.Configuration;

            UpdateWorkflowIndicators();
        }

        protected override void OnStatusChanged()
        {
            // -> (currently nothing to do)
            base.OnStatusChanged();

            if (this.Initialized)
                UpdateWorkflowIndicators();
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

            UpdateWorkflowIndicators();
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

            UpdateWorkflowIndicators();
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
                    return null;
                case LibraryImporterWorkflowStep.FinalReport:
                    return null;
                default:
                    throw new Exception("Unhandled import step type");
            }
        }
        private void UpdateWorkflowIndicators()
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
                    this.WorkflowNextEnabled = !this.Loading && this.StagingWorkflow.StagedFiles.Any();
                    this.WorkflowPreviousEnabled = !this.Loading;
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
                    this.WorkflowNextEnabled = !this.Loading && !this.CompletionWorkflow.StagedFiles.Any(x => x.ImportOutput.ImportResult == LibraryWorkerResultLevel.None);
                    this.WorkflowPreviousEnabled = !this.Loading;
                    break;

                // Validation: TODO
                //
                case LibraryImporterWorkflowStep.ImportCompletion:
                    this.WorkflowNextEnabled = !this.Loading;
                    this.WorkflowPreviousEnabled = !this.Loading;
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

        public override bool CanExecute()
        {
            return this.ComponentParts.Any(x => x.CanExecute());
        }
        public override bool CanReset()
        {
            return this.ComponentParts.Any(x => x.CanReset());
        }
        public override bool CanLoad()
        {
            return this.ComponentParts.Any(x => x.CanLoad());
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
            UpdateWorkflowIndicators();
        }
    }
}
