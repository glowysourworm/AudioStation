using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

using AudioStation.Core.Component;
using AudioStation.ViewModels;

namespace AudioStation.Views
{
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
            var oldViewModel = e.OldValue as LibraryLoaderViewModel;
            var newViewModel = e.NewValue as LibraryLoaderViewModel;

            if (oldViewModel != null)
                oldViewModel.PropertyChanged -= OnMainViewModelPropertyChanged;

            if (newViewModel != null)
                newViewModel.PropertyChanged += OnMainViewModelPropertyChanged;
        }

        private void OnMainViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryLoaderViewModel;

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
            var viewModel = this.DataContext as LibraryLoaderViewModel;

            if (viewModel != null)
            {
                switch (viewModel.LibraryLoaderState)
                {
                    case PlayStopPause.Play:
                        //this.PlayRB.IsChecked = true;
                        break;
                    case PlayStopPause.Pause:
                        //this.PauseRB.IsChecked = true;
                        break;
                    case PlayStopPause.Stop:
                       // this.StopRB.IsChecked = true;
                        break;
                    default:
                        throw new Exception("Unhandled LibraryLoaderState:  LibraryLoaderView");
                }
            }
        }

        private void PauseRB_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryLoaderViewModel;

            if (viewModel != null)
            {
                viewModel.LibraryLoaderState = PlayStopPause.Pause;
            }
        }

        private void StopRB_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryLoaderViewModel;

            if (viewModel != null)
            {
                viewModel.LibraryLoaderState = PlayStopPause.Stop;
            }
        }

        private void PlayRB_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryLoaderViewModel;

            if (viewModel != null)
            {
                viewModel.LibraryLoaderState = PlayStopPause.Play;
            }
        }
    }
}
