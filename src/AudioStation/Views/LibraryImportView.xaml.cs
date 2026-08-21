using System.Windows;
using System.Windows.Controls;

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

        LibraryImporterViewModel _viewModel;

        public LibraryImportView()
        {
            InitializeComponent();

            this.DataContextChanged += LibraryImportView_DataContextChanged;
        }

        [IocImportingConstructor]
        public LibraryImportView(IIocRegionManager regionManager)
        {
            InitializeComponent();

            this.DataContextChanged += LibraryImportView_DataContextChanged;

            _regionManager = regionManager;
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

        /// <summary>
        /// Runs initial process just after loading the view
        /// </summary>
        private void InitializeImportStep(Type viewType)
        {
            // Configuration
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                // TODO
            }

            // Staging
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                // TODO
            }

            // Tag Completion 
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                // Run Acoust ID -> Music Brainz (cache results)
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
            }

            //return Task.CompletedTask;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel == null)
                return;

            RefreshFromDataContext(viewModel);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            // Configuration
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                // Nothing to do
            }

            // Staging 
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                LoadImportView(typeof(LibraryImportConfigurationView), true, true);
            }

            // Tag Completion
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                LoadImportView(typeof(LibraryImportStagingView), true, true);
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
                LoadImportView(typeof(LibraryImportTagCompletionView), true, true);
            }

            RefreshFromDataContext(this.DataContext as LibraryImporterViewModel);
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Staging
            if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportConfigurationView)
            {
                LoadImportView(typeof(LibraryImportStagingView), false, true);
                InitializeImportStep(typeof(LibraryImportStagingView));
            }

            // Tag Completion
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportStagingView)
            {
                LoadImportView(typeof(LibraryImportTagCompletionView), false, true);
                InitializeImportStep(typeof(LibraryImportTagCompletionView));
            }

            // Configuration
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportTagCompletionView)
            {
                LoadImportView(typeof(LibraryImportFinalView), false, true);
                InitializeImportStep(typeof(LibraryImportFinalView));
            }

            // Final View (User can go back as long as they haven't pressed "Execute")
            else if (_regionManager.GetRegion("LibraryImporterControlRegion").Content is LibraryImportFinalView)
            {
                // Nothing to do
            }

            RefreshFromDataContext(this.DataContext as LibraryImporterViewModel);
        }
    }
}
