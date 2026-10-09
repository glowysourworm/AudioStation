using System.Windows;
using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Core.Component.LibraryLoaderComponent;
using AudioStation.ViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels;
using AudioStation.ViewModels.ServiceComponent.LibraryLoaderViewModels.Interface;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibraryLoaderWorkerView : UserControl
    {
        private readonly IDialogController _dialogController;

        [IocImportingConstructor]
        public LibraryLoaderWorkerView(IDialogController dialogController)
        {
            _dialogController = dialogController;

            InitializeComponent();

            this.DataContextChanged += LibraryLoaderWorkerView_DataContextChanged;
        }

        private void LibraryLoaderWorkerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldVM = e.OldValue as ILibraryLoaderWorkerViewModel;
            var newVM = e.NewValue as ILibraryLoaderWorkerViewModel;

            if (oldVM != null)
            {
                oldVM.WorkItemChangedEvent -= ServiceWorkflow_WorkItemChangedEvent;
                oldVM.StatusChangeEvent -= ServiceWorkflow_StatusChangeEvent;
            }
            if (newVM != null)
            {
                newVM.WorkItemChangedEvent += ServiceWorkflow_WorkItemChangedEvent;
                newVM.StatusChangeEvent += ServiceWorkflow_StatusChangeEvent;
            }

            UpdateViewContext();
        }

        private void UpdateViewContext()
        {
            var viewModel = this.DataContext as ILibraryLoaderWorkerViewModel;

            if (viewModel != null)
            {
                this.ExecuteButton.IsEnabled = viewModel.CanChangeLoaderState(PlayStopPause.Play);
                this.PauseButton.IsEnabled = viewModel.CanChangeLoaderState(PlayStopPause.Pause);
                this.CancelButton.IsEnabled = viewModel.CanChangeLoaderState(PlayStopPause.Stop);

                this.ExecuteButton.IsChecked = viewModel.LibraryLoaderState == PlayStopPause.Play;
                this.CancelButton.IsChecked = viewModel.LibraryLoaderState == PlayStopPause.Stop;
                this.PauseButton.IsChecked = viewModel.LibraryLoaderState == PlayStopPause.Pause;

                // Single Execution
                if (!viewModel.ExecuteAsBulk)
                    this.LoaderWorkItemsLB.ItemsSource = viewModel.WorkItems;

                // Bulk Execution
                else
                {
                    var bulkWorkItem = viewModel.BulkWorkItems
                                                .FirstOrDefault(x => x.State != LibraryWorkItemState.Successful);

                    if (bulkWorkItem != null && this.LoaderWorkItemsLB.ItemsSource != bulkWorkItem.WorkItemsCompleted)
                        this.LoaderWorkItemsLB.ItemsSource = bulkWorkItem.WorkItemsCompleted;
                }
            }
        }

        private void ScrollIntoView(ILibraryLoaderWorkerViewModel sender, LibraryWorkItemViewModel item)
        {
            // Scroll the item into view
            this.LoaderWorkItemsLB.ScrollIntoView(item);

            // An exception occurs when the dialog window is open. There may be a way around the exception; but
            // it doesn't yet make sense.. something to do with other data binding to the work items
            if (!_dialogController.IsShowing())
            {
                this.LoaderWorkItemsLB.SelectedItem = item;
            }
        }
        private void ServiceWorkflow_StatusChangeEvent(ServiceComponentPartViewModelBase sender, bool working, bool loaded)
        {
            UpdateViewContext();
        }

        private void ServiceWorkflow_WorkItemChangedEvent(ILibraryLoaderWorkerViewModel sender, LibraryWorkItemViewModel item)
        {
            UpdateViewContext();
            ScrollIntoView(sender, item);
        }
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ILibraryLoaderWorkerViewModel;

            if (viewModel != null)
            {
                viewModel.ChangeLoaderState(PlayStopPause.Play);
            }
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ILibraryLoaderWorkerViewModel;

            if (viewModel != null)
            {
                viewModel.ChangeLoaderState(PlayStopPause.Pause);
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ILibraryLoaderWorkerViewModel;

            if (viewModel != null)
            {
                viewModel.ChangeLoaderState(PlayStopPause.Stop);
            }
        }

        //private void LoaderWorkItemsLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        //{
        //    var item = this.LoaderWorkItemsLB.SelectedItem as LibraryWorkItemViewModel;

        //    if (item != null)
        //    {
        //        this.IsEnabled = false;

        //        _dialogController.ShowDialogWindowSync(new DialogEventData(item));

        //        this.IsEnabled = true;
        //    }
        //}
    }
}
