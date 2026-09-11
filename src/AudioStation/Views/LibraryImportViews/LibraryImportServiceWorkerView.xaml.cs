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

            UpdateViewContext();
        }

        private void UpdateViewContext()
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

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateViewContext();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}