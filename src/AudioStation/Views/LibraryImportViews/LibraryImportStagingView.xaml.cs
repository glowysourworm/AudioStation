using System.Windows.Controls;

using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.UI.Controls.TreeViewUI;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportStagingView : UserControl
    {
        private readonly ILibraryLoaderService _libraryLoaderService;

        [IocImportingConstructor]
        public LibraryImportStagingView(ILibraryLoaderService libraryLoaderService)
        {
            _libraryLoaderService = libraryLoaderService;

            InitializeComponent();
        }
        private void ImportTV_SelectedItemsChanged(SimpleTreeView treeView, IEnumerable<TreeViewModelBase> selectedItems)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                viewModel.Staging.SelectedFileCount = selectedItems.Count(x => !x.CanHaveChildren);
            }
        }

        private void StagedLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                // Selection Counts
                viewModel.Staging.StagedSelectedCount = viewModel.Staging.StagedFiles.Count(x => x.IsSelected);
                viewModel.Staging.LibraryConflictCount = viewModel.Staging.StagedFiles.Count(x => x.LibraryConflict);
            }
        }
    }
}
