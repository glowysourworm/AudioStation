using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportServiceWorkerView : UserControl
    {
        [IocImportingConstructor]
        public LibraryImportServiceWorkerView()
        {
            InitializeComponent();
        }
    }
}