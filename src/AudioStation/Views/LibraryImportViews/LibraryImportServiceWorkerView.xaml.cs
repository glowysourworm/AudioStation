using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

using AudioStation.ViewModels.ComponentViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportServiceWorkerView : UserControl
    {
        public LibraryImportServiceWorkerView()
        {
            InitializeComponent();

            this.DataContextChanged += LibraryImportLoaderView_DataContextChanged;
        }

        private void LibraryImportLoaderView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldVM = e.OldValue as LibraryImporterViewModel;
            var newVM = e.NewValue as LibraryImporterViewModel;

            if (oldVM != null)
                oldVM.ServiceWorkflow.PropertyChanged -= OnViewModelPropertyChanged;

            if (newVM != null)
                newVM.ServiceWorkflow.PropertyChanged += OnViewModelPropertyChanged;

            InitializeViewContext();
            UpdateViewContext();
        }

        private void InitializeViewContext()
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                this.LoaderLB.Items.Clear();
                this.LoaderLB.Items.Add(viewModel.ServiceWorkflow.AcoustIDWorker);
                this.LoaderLB.Items.Add(viewModel.ServiceWorkflow.MusicBrainzBasicWorker);
                this.LoaderLB.Items.Add(viewModel.ServiceWorkflow.MusicBrainzAlbumArtWorker);
            }
        }

        private void UpdateViewContext()
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                this.ExecuteButton.IsEnabled = viewModel.ServiceWorkflow.CanExecute();
                this.PauseButton.IsEnabled = !viewModel.ServiceWorkflow.CanExecute();
                this.CancelButton.IsEnabled = !viewModel.ServiceWorkflow.CanExecute();
            }
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateViewContext();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                if (viewModel.ServiceWorkflow.CanExecute())
                {
                    // No need for loading window (these are async tasks)
                    viewModel.ServiceWorkflow.Execute((x, y, z, w) => { });
                }

            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {

            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {

            }
        }
    }
}