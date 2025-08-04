using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using AudioStation.ViewModels.LibraryLoaderViewModels.Import;

using SimpleWpf.IocFramework.Application.Attribute;

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
            //var viewModel = this.DataContext as LibraryLoaderImportViewModel;
            //var selectedViewModel = (e.OriginalSource as FrameworkElement).DataContext as LibraryLoaderImportTreeViewModel;

            //if (viewModel != null)
            //{
            //    // Reset Selection
            //    viewModel.SourceDirectory.RecurseForEach(node =>
            //    {
            //        // Shared Directory
            //        node.NodeValue.IsSelected = selectedViewModel.Parent == node.Parent;
            //    });
            //}
        }

        private void InputFileExpanderButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                viewModel.SourceDirectory.RecurseForEach(item =>
                {
                    item.NodeValue.IsExpanded = (selectedFile == item.NodeValue) && selectedFile.IsExpanded;
                });
            }
        }
    }
}
