using System.Windows;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.ViewModels.ServiceComponent;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels.Workflow;
using AudioStation.Views.LibraryImportViews;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.RegionManagement.Interface;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibraryImportView : UserControl
    {
        public static readonly DependencyProperty PreviousStepReadyProperty =
            DependencyProperty.Register("PreviousStepReady", typeof(bool), typeof(LibraryImportView));

        public bool PreviousStepReady
        {
            get { return (bool)GetValue(PreviousStepReadyProperty); }
            set { SetValue(PreviousStepReadyProperty, value); }
        }

        private readonly IIocRegionManager _regionManager;
        private readonly IDialogController _dialogController;

        private readonly IAudioStationComponentController _componentViewModelLoader;

        LibraryImporterViewModel _viewModel;

        public LibraryImportView()
        {
            InitializeComponent();

            this.DataContextChanged += LibraryImportView_DataContextChanged;
        }

        [IocImportingConstructor]
        public LibraryImportView(IIocRegionManager regionManager, IDialogController dialogController, IAudioStationComponentController componentViewModelLoader)
        {
            InitializeComponent();

            _regionManager = regionManager;
            _dialogController = dialogController;
            _componentViewModelLoader = componentViewModelLoader;

            this.DataContextChanged += LibraryImportView_DataContextChanged;
        }
        private void LibraryImportView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                _viewModel = e.NewValue as LibraryImporterViewModel;
        }

        private void LoadImportView(Type viewType, bool previous, bool ignoreTransition)
        {
            _regionManager.LoadNamedInstance("LibraryImporterControlRegion", viewType, ignoreTransition);
        }

        private bool ConfirmImportStep(LibraryImporterWorkflowStep step)
        {
            switch (step)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    return true;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                {
                    // Run Acoust ID -> Music Brainz (cache results)
                    if (_dialogController.ShowConfirmation("Continue to Configuration Options?",
                        string.Format("You have chosen import type:  {0}", _viewModel.WorkflowConfiguration.ImportType),
                        "",
                        "Are you ready to proceed?"))
                    {
                        return true;
                    }
                    else
                        return false;
                }
                case LibraryImporterWorkflowStep.Staging:
                {
                    // Run Acoust ID -> Music Brainz (cache results)
                    if (_dialogController.ShowConfirmation("Continue to Staging?",
                        "You have now completed the import configuration.",
                        "",
                        "It is STRONGLY RECOMMENDED that you backup your files!",
                        "",
                        "Are you ready to proceed?"))
                    {
                        return true;
                    }
                    else
                        return false;
                }
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    return true;
                case LibraryImporterWorkflowStep.TagCompletion:
                {
                    // Run Acoust ID -> Music Brainz (cache results)
                    if (_dialogController.ShowConfirmation("Continue to Finalize Import?",
                        "Your current tag information will be imported for {0} tags",
                        "",
                        "These tracks will be imported into your library. However, you can",
                        "come back later on to revisit this import and complete tags that",
                        "have not yet been completed:  ({1} tags)",
                        "",
                        "You may also change your library data at any time using Audio Station's",
                        "Library Maintainence features - which essentially allow you to detail",
                        "you library tracks using Music Brainz, LastFm, and other available data",
                        "services at any time.",
                        "",
                        "Are you ready to finalize your import?"))
                    {
                        return true;
                    }
                    else
                        return false;
                }
                case LibraryImporterWorkflowStep.ImportCompletion:
                    return true;
                case LibraryImporterWorkflowStep.FinalReport:
                    return true;
                default:
                    throw new Exception("Unhandled import step type");
            }
        }

        private void PostLoadImportStep(LibraryImporterWorkflowStep step)
        {
            // Procedure: The component parts are loaded using a top-down controller
            //            design. So, we need to invoke the load method on the component
            //            controller. The view model will tell us which component part is
            //            needed for the current step. 
            //
            //            We can chose when to load this component part based on how
            //            we want to handle the view loading..
            //

            var componentPart = _viewModel.GetWorkflowComponentPart(step);

            // Not every step has a component part
            if (componentPart != null &&
                componentPart.CanLoad())
                _componentViewModelLoader.LoadComponent<LibraryImporterViewModel>(true, componentPart.Id);
        }

        private void PreLoadImportStep(LibraryImporterWorkflowStep step)
        {
            switch (step)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    break;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    break;
                case LibraryImporterWorkflowStep.Staging:
                    break;
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    break;
                case LibraryImporterWorkflowStep.TagCompletion:
                    break;
                case LibraryImporterWorkflowStep.ImportCompletion:
                    break;
                case LibraryImporterWorkflowStep.FinalReport:
                    break;
                default:
                    throw new Exception("Unhandled import step type");
            }
        }

        private void CompleteImportStep(LibraryImporterWorkflowStep step)
        {
            switch (step)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    // Save Current Workflow
                    //_viewModel.SaveCurrentWorkflow();
                    break;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    // Save Current Workflow
                    //_viewModel.SaveCurrentWorkflow();
                    break;
                case LibraryImporterWorkflowStep.Staging:
                    break;
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    break;
                case LibraryImporterWorkflowStep.TagCompletion:
                    break;
                case LibraryImporterWorkflowStep.ImportCompletion:
                    break;
                case LibraryImporterWorkflowStep.FinalReport:
                    break;
                default:
                    throw new Exception("Unhandled import step type");
            }
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_viewModel.WorkflowCurrentStep)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    // Nothing to do
                    break;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    MoveToImportStep<LibraryImportConfigurationView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.Configuration, true);
                    break;
                case LibraryImporterWorkflowStep.Staging:
                    MoveToImportStep<LibraryImportConfigurationOptionsView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.ConfigurationOptions, true);
                    break;
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    MoveToImportStep<LibraryImportStagingView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.Staging, true);
                    break;
                case LibraryImporterWorkflowStep.TagCompletion:
                    MoveToImportStep<LibraryImportServiceWorkerView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.ServiceWorkers, true);
                    break;
                case LibraryImporterWorkflowStep.ImportCompletion:
                    MoveToImportStep<LibraryImportTagCompletionView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.TagCompletion, true);
                    break;
                case LibraryImporterWorkflowStep.FinalReport:
                    MoveToImportStep<LibraryImportCompletionView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.ImportCompletion, true);
                    break;
                default:
                    throw new Exception("Unhandled workflow step");
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_viewModel.WorkflowCurrentStep)
            {
                case LibraryImporterWorkflowStep.Configuration:
                    MoveToImportStep<LibraryImportConfigurationOptionsView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.ConfigurationOptions, false);
                    break;
                case LibraryImporterWorkflowStep.ConfigurationOptions:
                    MoveToImportStep<LibraryImportStagingView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.Staging, false);
                    break;
                case LibraryImporterWorkflowStep.Staging:
                    MoveToImportStep<LibraryImportServiceWorkerView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.ServiceWorkers, false);
                    break;
                case LibraryImporterWorkflowStep.ServiceWorkers:
                    MoveToImportStep<LibraryImportTagCompletionView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.TagCompletion, false);
                    break;
                case LibraryImporterWorkflowStep.TagCompletion:
                    MoveToImportStep<LibraryImportFinalReportView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.ImportCompletion, false);
                    break;
                case LibraryImporterWorkflowStep.ImportCompletion:
                    MoveToImportStep<LibraryImportFinalReportView>(_viewModel.WorkflowCurrentStep, LibraryImporterWorkflowStep.FinalReport, false);
                    break;
                case LibraryImporterWorkflowStep.FinalReport:
                    // Nothing to do
                    break;
                default:
                    throw new Exception("Unhandled workflow step");
            }
        }

        private void MoveToImportStep<TView>(LibraryImporterWorkflowStep from, LibraryImporterWorkflowStep to, bool isPrevious)
        {
            var view = typeof(TView);

            if (isPrevious)
            {
                LoadImportView(view, isPrevious, true);

                // Workflow!
                _viewModel.WorkflowPrevious();
            }
            else if (ConfirmImportStep(from))
            {
                // From
                CompleteImportStep(from);

                // To
                PreLoadImportStep(to);
                LoadImportView(view, isPrevious, true);
                PostLoadImportStep(to);

                // Workflow!
                _viewModel.WorkflowNext();
            }
        }
    }
}
