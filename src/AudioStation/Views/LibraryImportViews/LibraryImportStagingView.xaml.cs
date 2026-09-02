using System.ComponentModel;
using System.Windows.Controls;

using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.UI.ViewModel.FileTreeView;
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

        private void OnImportTreeChanged(TreeViewModelBase<FileTreeNodeViewModel> treeSender, FileTreeNodeViewModel item, PropertyChangedEventArgs eventArgs)
        {
            var viewModel = this.DataContext as LibraryImporterViewModel;
            var itemViewModel = treeSender as LibraryImporterTreeViewModel;

            if (viewModel != null && itemViewModel != null && itemViewModel.NodeValue == item)
            {
                if (item.IsDirectory &&
                    item.IsExpanded &&
                   !item.IsLoaded)
                {
                    _libraryLoaderService.LoadImporterTreeNextDepth(itemViewModel, viewModel.Options.SourceDirectory, viewModel.Options.DestinationDirectory, "*.mp3", viewModel.Options);
                }
            }
        }
    }
}
