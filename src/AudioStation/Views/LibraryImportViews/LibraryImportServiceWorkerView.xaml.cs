using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component;
using AudioStation.Event;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Interface;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportServiceWorkerView : UserControl
    {
        private readonly IIocEventAggregator _eventAggregator;
        private readonly IDialogController _dialogController;

        [IocImportingConstructor]
        public LibraryImportServiceWorkerView(IIocEventAggregator eventAggregator, IDialogController dialogController)
        {
            _eventAggregator = eventAggregator;
            _dialogController = dialogController;

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
                oldVM.ServiceWorkflow.PropertyChanged -= ServiceWorkflow_PropertyChanged;
            }
            if (newVM != null)
            {
                newVM.ServiceWorkflow.WorkItemChangedEvent += ServiceWorkflow_WorkItemChangedEvent;
                newVM.ServiceWorkflow.StatusChangeEvent += ServiceWorkflow_StatusChangeEvent;
                newVM.ServiceWorkflow.PropertyChanged += ServiceWorkflow_PropertyChanged;
            }


            UpdateViewContext();
        }
        private void UpdateViewContext()
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                this.ExecuteButton.IsEnabled = viewModel.ServiceWorkflow.CanChangeLoaderState(PlayStopPause.Play);
                this.PauseButton.IsEnabled = viewModel.ServiceWorkflow.CanChangeLoaderState(PlayStopPause.Pause);
                this.CancelButton.IsEnabled = viewModel.ServiceWorkflow.CanChangeLoaderState(PlayStopPause.Stop);

                this.ExecuteButton.IsChecked = viewModel.ServiceWorkflow.LibraryLoaderState == PlayStopPause.Play;
                this.CancelButton.IsChecked = viewModel.ServiceWorkflow.LibraryLoaderState == PlayStopPause.Stop;
                this.PauseButton.IsChecked = viewModel.ServiceWorkflow.LibraryLoaderState == PlayStopPause.Pause;
            }
        }

        private void ScrollIntoView(ILibraryLoaderWorkerViewModel sender, LibraryWorkItemViewModel item)
        {
            // Select the workflow item from the sender
            this.LoaderLB.SelectedItem = sender;

            // Scroll the item into view
            this.LoaderWorkItemsLB.ScrollIntoView(item);

            // An exception occurs when the dialog window is open. There may be a way around the exception; but
            // it doesn't yet make sense.. something to do with other data binding to the work items
            if (!_dialogController.IsShowing())
            {
                this.LoaderWorkItemsLB.SelectedItem = item;
            }
        }
        private void ServiceWorkflow_StatusChangeEvent(ILibraryLoaderWorkerViewModel sender, bool isWorking)
        {
            UpdateViewContext();
        }

        private void ServiceWorkflow_WorkItemChangedEvent(ILibraryLoaderWorkerViewModel sender, LibraryWorkItemViewModel item)
        {
            UpdateViewContext();
            ScrollIntoView(sender, item);
        }
        private void ServiceWorkflow_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            UpdateViewContext();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                viewModel.ServiceWorkflow.ChangeLoaderState(PlayStopPause.Play);
            }
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                viewModel.ServiceWorkflow.ChangeLoaderState(PlayStopPause.Pause);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                viewModel.ServiceWorkflow.ChangeLoaderState(PlayStopPause.Stop);
            }
        }

        private void LoaderWorkItemsLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = this.LoaderWorkItemsLB.SelectedItem as LibraryWorkItemViewModel;

            if (item != null)
            {
                this.IsEnabled = false;

                _dialogController.ShowDialogWindowSync(new DialogEventData(item));

                this.IsEnabled = true;
            }
        }
    }
}