using System.Windows.Controls;

using AudioStation.Controls;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import;

using SimpleWpf.IocFramework.Application.Attribute;

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

        private void ImportTV_ItemExpandedEvent(object sender, bool isExpanded)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;
            var directoryTree = (LibraryImporterTreeViewModel)sender;

            if (directoryTree != null && viewModel != null)
            {
                if (directoryTree.NodeValue.IsDirectory &&
                   !directoryTree.NodeValue.IsLoaded &&
                    isExpanded)
                {
                    _libraryLoaderService.LoadImporterTreeNextDepth(ref directoryTree, viewModel.Options.SourceDirectory, viewModel.Options.DestinationDirectory, "*.mp3", viewModel.Options);
                }
            }
        }
    }
}
