using System.Windows.Controls;

using AudioStation.ViewModels.ServiceComponent;
using AudioStation.ViewModels.ServiceComponent.LibraryImporterViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.UI.Controls.TreeViewUI;
using SimpleWpf.UI.ViewModel.TreeView;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportTagCompletionView : UserControl
    {
        [IocImportingConstructor]
        public LibraryImportTagCompletionView()
        {
            InitializeComponent();
        }

        private void StagedFileLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Unrealized items have no binding, so selection changes are reflected to the
            // data here. e.AddedItems / e.RemovedItems contain every changed item,
            // regardless of container realization state.
            foreach (LibraryImporterFileTreeNodeViewModel item in e.AddedItems)
            {
                item.IsSelected = true;
            }

            foreach (LibraryImporterFileTreeNodeViewModel item in e.RemovedItems)
            {
                item.IsSelected = false;
            }

            // Trigger Command Updates
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                viewModel.CompletionWorkflow.UpdateCommands();
            }
        }

        private void StagedFileTV_SelectedItemsChanged(SimpleTreeView sender, IEnumerable<TreeViewNodeModelBase> selectedItems)
        {
            // Trigger Command Updates
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null)
            {
                viewModel.CompletionWorkflow.UpdateCommands();
            }
        }
    }
}
