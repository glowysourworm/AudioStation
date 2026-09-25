using System.Windows.Controls;

using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

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

            // TODO: The following code will need to be put with an implementation 
            //       for virtualizing list box + the selection view model should
            //       be part of the ViewModel inheritance hierarchy.

            // Unrealized items have no binding, so selection changes are reflected to the
            // data here. e.AddedItems / e.RemovedItems contain every changed item,
            // regardless of container realization state.
            foreach (LibraryImporterFileViewModel item in e.AddedItems)
            {
                item.IsSelected = true;
            }

            foreach (LibraryImporterFileViewModel item in e.RemovedItems)
            {
                item.IsSelected = false;
            }
        }
    }
}
