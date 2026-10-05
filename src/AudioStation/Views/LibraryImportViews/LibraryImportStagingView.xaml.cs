using System.Windows.Controls;

using AudioStation.Service.Interface;

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

        private void StagedLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //// TODO: The following code will need to be put with an implementation 
            ////       for virtualizing list box + the selection view model should
            ////       be part of the ViewModel inheritance hierarchy.

            //// Unrealized items have no binding, so selection changes are reflected to the
            //// data here. e.AddedItems / e.RemovedItems contain every changed item,
            //// regardless of container realization state.
            //foreach (LibraryImporterFileTreeNodeViewModel item in e.AddedItems)
            //{
            //    item.IsSelected = true;
            //}

            //foreach (LibraryImporterFileTreeNodeViewModel item in e.RemovedItems)
            //{
            //    item.IsSelected = false;
            //}

            //// Trigger Command Updates
            //var viewModel = this.DataContext as LibraryImporterViewModel;

            //if (viewModel != null)
            //{
            //    viewModel.StagingWorkflow.UpdateCommands();
            //}
        }
    }
}
