using System.Windows;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.Views.LibraryImportViews;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.RegionManagement.Interface;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibraryImportView : UserControl
    {
        public static readonly DependencyProperty NextStepReadyProperty =
            DependencyProperty.Register("NextStepReady", typeof(bool), typeof(LibraryImportView));

        public static readonly DependencyProperty PreviousStepReadyProperty =
            DependencyProperty.Register("PreviousStepReady", typeof(bool), typeof(LibraryImportView));

        public static readonly DependencyProperty ExecuteReadyProperty =
            DependencyProperty.Register("ExecuteReady", typeof(bool), typeof(LibraryImportView));

        public bool NextStepReady
        {
            get { return (bool)GetValue(NextStepReadyProperty); }
            set { SetValue(NextStepReadyProperty, value); }
        }
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
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel == null)
                return;

            RefreshFromDataContext(viewModel);
        }
        private void LibraryImportView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Unhook
            if (e.OldValue != null)
            {
                var viewModel = e.OldValue as LibraryImporterViewModel;

                if (viewModel != null)
                {
                    viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                }
            }

            // Hook
            if (e.NewValue != null)
            {
                _viewModel = e.NewValue as LibraryImporterViewModel;

                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                    // Initialize Buttons
                    RefreshFromDataContext(_viewModel);
                }
            }
        }

        private bool AreStagingRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }
        private bool AreTagCompletionRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }
        private bool AreWorkflowSelectionRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }
        private bool AreConfigurationRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }
        private bool AreConfigurationOptionsRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }
        private bool AreImportLoaderRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }
        private bool AreFinalRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }

        private void RefreshFromDataContext(LibraryImporterViewModel viewModel)
        {
            // Workflow Selection
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportWorkflowSelectionView)
            {
                this.NextStepReady = AreWorkflowSelectionRequirementsMet(viewModel);
                this.PreviousStepReady = false;
            }

            // Configuration
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                this.NextStepReady = AreConfigurationRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }

            // Configuration Options
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationOptionsView)
            {
                this.NextStepReady = AreConfigurationOptionsRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }

            // Staging
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                this.NextStepReady = AreStagingRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }

            // Import Loader
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportLoaderView)
            {
                this.NextStepReady = AreImportLoaderRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }

            // Tag Completion 
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                this.NextStepReady = AreTagCompletionRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
                this.NextStepReady = AreFinalRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }

            else
                throw new Exception("Unhandled region view type");
        }

        private void LoadImportView(Type viewType, bool previous, bool ignoreTransition)
        {
            _regionManager.LoadNamedInstance("LibraryImporterControlRegion", viewType, ignoreTransition);
        }

        private bool ConfirmImportStep(Type viewType)
        {
            // Workflow Selection
            if (viewType == typeof(LibraryImportWorkflowSelectionView))
            {
                if (_dialogController.ShowConfirmation("Continue to Configuration?",
                    string.Format("You have chosen workflow:  {0}", _viewModel.Workflow.Name),
                    "",
                    "Are you ready to proceed?"))
                {
                    return true;
                }
                else
                    return false;
            }

            // Configuration
            if (viewType == typeof(LibraryImportConfigurationView))
            {
                if (_dialogController.ShowConfirmation("Continue to Configuration Options?",
                    string.Format("You have chosen import type:  {0}", _viewModel.Workflow.Configuration.ImportDirectory.ImportType),
                    "",
                    "Are you ready to proceed?"))
                {
                    return true;
                }
                else
                    return false;
            }

            // Configuration Options
            else if (viewType == typeof(LibraryImportConfigurationOptionsView))
            {
                // Run Acoust ID -> Music Brainz (cache results)
                if (_dialogController.ShowConfirmation("Continue to Configuration Options?",
                    string.Format("You have chosen import type:  {0}", _viewModel.Workflow.Configuration.ImportDirectory.ImportType),
                    "",
                    "Are you ready to proceed?"))
                {
                    return true;
                }
                else
                    return false;
            }

            // Staging -> Import Loader
            else if (viewType == typeof(LibraryImportStagingView))
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

            // Import Loader -> Tag Completion
            else if (viewType == typeof(LibraryImportLoaderView))
            {
                return true;
            }

            // Tag Completion -> Finalize
            else if (viewType == typeof(LibraryImportTagCompletionView))
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

            // Final View
            else if (viewType == typeof(LibraryImportFinalView))
            {
                return true;
            }
            else
                throw new Exception("Unhandled view type");
        }

        private void InitializeImportStep(Type viewType)
        {
            // Workflow Selection
            if (viewType == typeof(LibraryImportWorkflowSelectionView))
            {
                // Nothing to do
            }

            // Configuration
            else if (viewType == typeof(LibraryImportConfigurationView))
            {
            }

            // Configuration Options
            else if (viewType == typeof(LibraryImportConfigurationOptionsView))
            {

            }

            // Staging
            else if (viewType == typeof(LibraryImportStagingView))
            {
                // This gets moved to workflow loader (probably just use the view model to call the sub-components)

                _componentViewModelLoader.LoadComponent<LibraryImporterViewModel>();
            }

            // Import Loader
            else if (viewType == typeof(LibraryImportLoaderView))
            {
                // This gets moved to workflow loader (probably just use the view model to call the sub-components)

                _componentViewModelLoader.LoadComponent<LibraryImporterViewModel>();
            }

            // Tag Completion
            else if (viewType == typeof(LibraryImportTagCompletionView))
            {

            }

            // Final View
            else if (viewType == typeof(LibraryImportFinalView))
            {

            }
            else
                throw new Exception("Unhandled view type");
        }

        private void CompleteImportStep(Type viewType)
        {
            // Workflow Selection
            if (viewType == typeof(LibraryImportWorkflowSelectionView))
            {
                // Nothing to do
            }

            // Configuration
            else if (viewType == typeof(LibraryImportConfigurationView))
            {
                // Save Current Workflow
                _viewModel.SaveCurrentWorkflow();
            }

            // Configuration Options
            else if (viewType == typeof(LibraryImportConfigurationOptionsView))
            {
                // Save Current Workflow
                _viewModel.SaveCurrentWorkflow();
            }

            // Staging
            else if (viewType == typeof(LibraryImportStagingView))
            {

            }

            // Import Loader
            else if (viewType == typeof(LibraryImportLoaderView))
            {
            }

            // Tag Completion
            else if (viewType == typeof(LibraryImportTagCompletionView))
            {

            }

            // Final View
            else if (viewType == typeof(LibraryImportFinalView))
            {

            }
            else
                throw new Exception("Unhandled view type");
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            // Workflow Selection
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportWorkflowSelectionView)
            {
                // Nothing to do
            }

            // Configuration
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                MoveToImportStep<LibraryImportConfigurationView, LibraryImportWorkflowSelectionView>(true);
            }

            // Configuration Options
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationOptionsView)
            {
                MoveToImportStep<LibraryImportConfigurationOptionsView, LibraryImportConfigurationView>(true);
            }

            // Staging 
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                MoveToImportStep<LibraryImportStagingView, LibraryImportConfigurationOptionsView>(true);
            }

            // Import Loader
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportLoaderView)
            {
                MoveToImportStep<LibraryImportLoaderView, LibraryImportStagingView>(true);
            }

            // Tag Completion
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                MoveToImportStep<LibraryImportTagCompletionView, LibraryImportLoaderView>(true);
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
                MoveToImportStep<LibraryImportFinalView, LibraryImportTagCompletionView>(true);
            }

            RefreshFromDataContext(this.DataContext as LibraryImporterViewModel);
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Workflow Selection
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportWorkflowSelectionView)
            {
                MoveToImportStep<LibraryImportWorkflowSelectionView, LibraryImportConfigurationView>(false);
            }

            // Configuration
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                MoveToImportStep<LibraryImportConfigurationView, LibraryImportConfigurationOptionsView>(false);
            }

            // Configuration Options
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationOptionsView)
            {
                MoveToImportStep<LibraryImportConfigurationOptionsView, LibraryImportStagingView>(false);
            }

            // Staging
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                MoveToImportStep<LibraryImportStagingView, LibraryImportLoaderView>(false);
            }

            // Import Loader
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportLoaderView)
            {
                MoveToImportStep<LibraryImportLoaderView, LibraryImportTagCompletionView>(false);
            }

            // Tag Completinon
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                MoveToImportStep<LibraryImportTagCompletionView, LibraryImportFinalView>(false);
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
                // Nothing to do
            }

            RefreshFromDataContext(this.DataContext as LibraryImporterViewModel);
        }

        private void MoveToImportStep<TFrom, TTo>(bool isPrevious)
        {
            var toView = typeof(TTo);
            var fromView = typeof(TFrom);

            if (isPrevious)
            {
                LoadImportView(toView, isPrevious, true);
            }
            else if (ConfirmImportStep(fromView))
            {
                CompleteImportStep(fromView);
                InitializeImportStep(toView);
                LoadImportView(toView, isPrevious, true);
            }
        }
    }
}
