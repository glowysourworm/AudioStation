using System.Windows.Controls;

using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

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
