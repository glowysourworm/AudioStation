using System.Windows;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.Views.LibraryImportViews;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.RegionManagement.Interface;
using SimpleWpf.Utilities;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibraryImportView : UserControl
    {
        public static readonly DependencyProperty NextStepReadyProperty =
            DependencyProperty.Register("NextStepReady", typeof(bool), typeof(LibraryImportView));

        public static readonly DependencyProperty PreviousStepReadyProperty =
            DependencyProperty.Register("PreviousStepReady", typeof(bool), typeof(LibraryImportView));

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

        LibraryImporterViewModel _viewModel;

        public LibraryImportView()
        {
            InitializeComponent();

            this.DataContextChanged += LibraryImportView_DataContextChanged;
        }

        [IocImportingConstructor]
        public LibraryImportView(IIocRegionManager regionManager, IDialogController dialogController)
        {
            InitializeComponent();

            this.DataContextChanged += LibraryImportView_DataContextChanged;

            _regionManager = regionManager;
            _dialogController = dialogController;
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
                var viewModel = e.NewValue as LibraryImporterViewModel;

                if (viewModel != null)
                {
                    viewModel.PropertyChanged += ViewModel_PropertyChanged;

                    // Initialize Buttons
                    RefreshFromDataContext(viewModel);
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
        private bool AreConfigurationRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }
        private bool AreFinalRequirementsMet(LibraryImporterViewModel viewModel)
        {
            return true;
        }

        private void RefreshFromDataContext(LibraryImporterViewModel viewModel)
        {
            _viewModel = viewModel;

            // Configuration
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                this.NextStepReady = AreConfigurationRequirementsMet(viewModel);
                this.PreviousStepReady = false;
            }

            // Staging
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                this.NextStepReady = AreStagingRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }

            // Tag Completion 
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                this.NextStepReady = AreTagCompletionRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
                this.NextStepReady = AreFinalRequirementsMet(viewModel);
                this.PreviousStepReady = true;
            }
        }

        private void LoadImportView(Type viewType, bool previous, bool ignoreTransition)
        {
            _regionManager.LoadNamedInstance("LibraryImporterControlRegion", viewType, ignoreTransition);
        }

        private bool ConfirmImportStep(Type viewType)
        {
            // Configuration
            if (viewType == typeof(LibraryImportConfigurationView))
            {
                // TODO
                return true;
            }

            // Staging
            else if (viewType == typeof(LibraryImportStagingView))
            {
                // TODO
                return true;
            }

            // Tag Completion 
            else if (viewType == typeof(LibraryImportTagCompletionView))
            {
                // Run Acoust ID -> Music Brainz (cache results)
                if (_dialogController.ShowConfirmation("Continue to Import?",
                    "This will begin the process of importing your Mp3's using the",
                    "AcoustID (and) Music Brainz services",
                    "",
                    "This may take some time... Are you ready to import?"))
                {
                    return true;
                }
                else
                    return false;
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            else if (viewType == typeof(LibraryImportFinalView))
            {
                return true;
            }
            else
                throw new Exception("Unhandled view type");
        }

        /// <summary>
        /// Runs initial process just after loading the view
        /// </summary>
        private async Task InitializeImportStep(Type viewType)
        {
            if (BasicHelpers.IsDispatcher() != ApplicationIsDispatcherResult.True)
                await BasicHelpers.InvokeDispatcher(InitializeImportStep, System.Windows.Threading.DispatcherPriority.Background, viewType);

            else
            {
                // Configuration
                if (viewType == typeof(LibraryImportConfigurationView))
                {
                    // TODO
                }

                // Staging
                else if (viewType == typeof(LibraryImportStagingView))
                {
                    // TODO
                }

                // Tag Completion 
                else if (viewType == typeof(LibraryImportTagCompletionView))
                {
                    // Procedure
                    //
                    // 1) Run AcoustID
                    // 2) Run Music Brainz (for all AcoustID entries)
                    // 3) Show result for best score
                    //
                }

                // Final View (User can go back as long as they haven't pressed "Execute")
                else if (viewType == typeof(LibraryImportFinalView))
                {
                }
                else
                    throw new Exception("Unhandled view type");
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel == null)
                return;

            RefreshFromDataContext(viewModel);
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            // Configuration
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                // Nothing to do
            }

            // Staging 
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                await MoveToImportStep<LibraryImportConfigurationView>(true);
            }

            // Tag Completion
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                await MoveToImportStep<LibraryImportStagingView>(true);
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
                await MoveToImportStep<LibraryImportTagCompletionView>(true);
            }

            RefreshFromDataContext(this.DataContext as LibraryImporterViewModel);
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Staging
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                await MoveToImportStep<LibraryImportStagingView>(false);
            }

            // Tag Completion
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                await MoveToImportStep<LibraryImportTagCompletionView>(false);
            }

            // Configuration
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                await MoveToImportStep<LibraryImportFinalView>(false);
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
                // Nothing to do
            }

            RefreshFromDataContext(this.DataContext as LibraryImporterViewModel);
        }

        private async Task MoveToImportStep<T>(bool isPrevious)
        {
            var viewType = typeof(T);

            if (ConfirmImportStep(viewType))
            {
                LoadImportView(viewType, isPrevious, true);
                await InitializeImportStep(viewType);
            }
        }
    }
}
