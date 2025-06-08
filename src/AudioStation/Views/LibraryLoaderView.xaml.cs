using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using AudioStation.Core.Component;
using AudioStation.ViewModels;

namespace AudioStation.Views
{
    /// <summary>
    /// Interaction logic for LibraryLoaderView.xaml
    /// </summary>
    public partial class LibraryLoaderView : UserControl
    {
        public LibraryLoaderView()
        {
            InitializeComponent();

            this.DataContextChanged += LibraryLoaderView_DataContextChanged;
            this.Loaded += (sender, e) =>
            {
                SetLoaderControlButtons();
            };
        }

        private void LibraryLoaderView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldViewModel = e.OldValue as MainViewModel;
            var newViewModel = e.NewValue as MainViewModel;

            if (oldViewModel != null)
                oldViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;

            if (newViewModel != null)
                newViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        }

        private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "LibraryLoaderState")
                    {
                        SetLoaderControlButtons();
                    }
                };
            }
        }

        private void SetLoaderControlButtons()
        {
            var viewModel = this.DataContext as MainViewModel;

            if (viewModel != null)
            {
                switch (viewModel.LibraryLoaderState)
                {
                    case PlayStopPause.Play:
                        this.PlayRB.IsChecked = true;
                        break;
                    case PlayStopPause.Pause:
                        this.PauseRB.IsChecked = true;
                        break;
                    case PlayStopPause.Stop:
                        this.StopRB.IsChecked = true;
                        break;
                    default:
                        throw new Exception("Unhandled LibraryLoaderState:  LibraryLoaderView");
                }
            }
        }

        private void PauseRB_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            if (viewModel != null)
            {
                viewModel.LibraryLoaderState = PlayStopPause.Pause;
            }
        }

        private void StopRB_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            if (viewModel != null)
            {
                viewModel.LibraryLoaderState = PlayStopPause.Stop;
            }
        }

        private void PlayRB_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;

            if (viewModel != null)
            {
                viewModel.LibraryLoaderState = PlayStopPause.Play;
            }
        }

        // This is the filter for library loader work items
        private void CollectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            var viewModel = this.DataContext as MainViewModel;
            var item = e.Item as LibraryWorkItemViewModel;

            if (viewModel != null && item != null)
            {
                if (item.LoadState == viewModel.SelectedLibraryWorkItemState)
                {
                    e.Accepted = true;
                    return;
                }
            }

            e.Accepted = false;
        }
    }
}
