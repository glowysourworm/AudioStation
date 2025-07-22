using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using AudioStation.Controller.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Event.DialogEvents;
using AudioStation.Model;
using AudioStation.ViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.Vendor.AcoustIDViewModel;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

using EMA.ExtendedWPFVisualTreeHelper;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views.LibraryLoaderViews
{
    [IocExportDefault]
    public partial class LibraryLoaderImportFileView : UserControl
    {
        [IocImportingConstructor]
        public LibraryLoaderImportFileView()
        {
            InitializeComponent();
        }

        private void ImportLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetSelection(false);
        }

        private void ImportLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SetSelection(true);
        }

        private void SetSelection(bool expand)
        {
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null)
            {
                foreach (var item in viewModel.SourceFiles)
                {
                    item.IsSelected = this.ImportLB.SelectedItems.Contains(item);

                    if (expand)
                        item.IsExpanded = !item.IsExpanded && item.IsSelected;
                }
            }
        }

        private void InputFileExpanderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                foreach (var item in viewModel.SourceFiles)
                {
                    item.IsExpanded = (selectedFile == item) && selectedFile.IsExpanded;
                }
            }
        }
    }
}
