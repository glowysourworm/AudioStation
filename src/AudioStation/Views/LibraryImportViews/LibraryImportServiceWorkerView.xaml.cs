using System.Windows;
using System.Windows.Controls;

using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;

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
            {
                oldVM.ServiceWorkflow.WorkItemChangedEvent -= ServiceWorkflow_WorkItemChangedEvent;
                oldVM.ServiceWorkflow.StatusChangeEvent -= ServiceWorkflow_StatusChangeEvent;
            }
            if (newVM != null)
            {
                newVM.ServiceWorkflow.WorkItemChangedEvent += ServiceWorkflow_WorkItemChangedEvent;
                newVM.ServiceWorkflow.StatusChangeEvent += ServiceWorkflow_StatusChangeEvent;
            }


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

        private void ScrollIntoView(LibraryLoaderWorkerViewModelBase sender, LibraryWorkItemViewModel item)
        {
            // Select the workflow item from the sender
            this.LoaderLB.SelectedItem = sender;

            // Scroll the item into view
            this.LoaderWorkItemsLB.ScrollIntoView(item);
        }
        private void ServiceWorkflow_StatusChangeEvent(LibraryLoaderWorkerViewModelBase sender, bool isWorking)
        {
            UpdateViewContext();
        }

        private void ServiceWorkflow_WorkItemChangedEvent(LibraryLoaderWorkerViewModelBase sender, LibraryWorkItemViewModel item)
        {
            UpdateViewContext();
            ScrollIntoView(sender, item);
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