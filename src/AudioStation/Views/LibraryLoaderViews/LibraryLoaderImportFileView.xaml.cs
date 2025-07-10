using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using AudioStation.Controller.Interface;
using AudioStation.ViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.Vendor.TagLibViewModel;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryLoaderViews
{
    [IocExportDefault]
    public partial class LibraryLoaderImportFileView : UserControl
    {
        private readonly IDialogController _dialogController;

        [IocImportingConstructor]
        public LibraryLoaderImportFileView(IDialogController dialogController)
        {
            _dialogController = dialogController;

            InitializeComponent();
        }

        private void InputLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null)
            {
                foreach (var item in viewModel.SourceFiles)
                {
                    item.IsSelected = this.InputLB.SelectedItems.Contains(item);
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

        private void EditTagButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                if (selectedFile != null)
                {
                    var tagFileGroupModel = new TagFileGroupViewModel(new TagFileViewModel[] { selectedFile.TagFile });

                    _dialogController.ShowTagWindow(tagFileGroupModel);
                }                
            }
        }

        private void AcoustIDTestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                // TODO
            }
        }

        private void MusicBrainzTestButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var viewModel = this.DataContext as LibraryLoaderImportViewModel;

            if (viewModel != null && button != null)
            {
                var selectedFile = button.DataContext as LibraryLoaderImportFileViewModel;

                // TODO
            }
        }
    }
}
