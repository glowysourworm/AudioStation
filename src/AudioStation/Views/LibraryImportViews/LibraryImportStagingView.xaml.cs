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
                viewModel.StagingWorkflow.SelectedFileCount = selectedItems.Count(x => !x.CanHaveChildren);
            }
        }

        private void StagedLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                // Selection Counts
                viewModel.StagingWorkflow.StagedSelectedCount = viewModel.StagingWorkflow.StagedFiles.Count(x => x.IsSelected);
                viewModel.StagingWorkflow.LibraryConflictCount = viewModel.StagingWorkflow.StagedFiles.Count(x => x.LibraryConflict);
            }
        }
    }
}
