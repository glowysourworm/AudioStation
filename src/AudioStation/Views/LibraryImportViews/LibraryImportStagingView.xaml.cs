using System.Windows.Controls;

using AudioStation.Controls;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryImporterViewModels.Import;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportStagingView : UserControl
    {
        [IocImportingConstructor]
        public LibraryImportStagingView()
        {
            InitializeComponent();
        }

        private void ImportTV_SelectedItemsChangedEvent(IEnumerable<MultiSelectTreeItemViewModel> selectedItems)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel == null)
                return;

            // This collection holds an internal binding. So, the selection flag doesn't get passed
            // on unless we force it to bind to our custom view model.

            viewModel.SourceDirectory.RecurseForEach(treeItem =>
            {
                if (treeItem.NodeValue is LibraryImporterFileViewModel)
                {
                    bool found = false;

                    foreach (var listBoxViewModel in selectedItems)
                    {
                        // Set IsSelected
                        var path = treeItem.NodeValue.ShortPath;
                        var otherPath = (string)listBoxViewModel.Item;

                        if (path == otherPath)
                        {
                            found = true;
                            break;
                        }
                    }

                    // Set Selection
                    treeItem.NodeValue.IsSelected = found;
                }
            });
        }
    }
}
