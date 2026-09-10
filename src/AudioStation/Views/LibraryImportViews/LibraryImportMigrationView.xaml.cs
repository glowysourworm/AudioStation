using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportMigrationView : UserControl
    {
        public LibraryImportMigrationView()
        {
            InitializeComponent();
        }
    }
}
