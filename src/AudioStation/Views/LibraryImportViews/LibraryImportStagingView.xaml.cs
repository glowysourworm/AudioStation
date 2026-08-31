using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;

using AudioStation.Controls;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.ViewModel;

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

            this.DataContextChanged += LibraryImportStagingView_DataContextChanged;
        }

        private void LibraryImportStagingView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            var newVM = e.NewValue as LibraryImporterViewModel;
            var oldVM = e.OldValue as LibraryImporterViewModel;

            if (oldVM != null)
                oldVM.SourceDirectory.ItemPropertyChangedTreeEvent -= OnImportTreeChanged;

            if (newVM != null)
                newVM.SourceDirectory.ItemPropertyChangedTreeEvent += OnImportTreeChanged;
        }

        private void OnImportTreeChanged(RecursiveDispatcherViewModel<PathViewModel> treeSender, PathViewModel nodeValue, PropertyChangedEventArgs eventArgs)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null && nodeValue != null)
            {
                if (nodeValue.IsDirectory &&
                    nodeValue.IsExpanded &&
                   !nodeValue.IsLoaded)
                {
                    _libraryLoaderService.LoadImporterTreeNextDepth(viewModel.SourceDirectory, viewModel.Options.SourceDirectory, viewModel.Options.DestinationDirectory, "*.mp3", viewModel.Options);
                }
            }
        }

        private void OnImportTreeChanged(RecursiveDispatcherViewModel<PathViewModel> treeSender, PathViewModel nodeValue, NotifyCollectionChangedEventArgs eventArgs)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;

            if (viewModel != null && nodeValue != null)
            {
                if (nodeValue.IsDirectory &&
                    nodeValue.IsExpanded &&
                   !nodeValue.IsLoaded)
                {
                    _libraryLoaderService.LoadImporterTreeNextDepth(viewModel.SourceDirectory, viewModel.Options.SourceDirectory, viewModel.Options.DestinationDirectory, "*.mp3", viewModel.Options);
                }
            }
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

        }
    }
}
