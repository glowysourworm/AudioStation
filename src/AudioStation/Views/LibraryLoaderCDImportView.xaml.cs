using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibraryLoaderCDImportView : UserControl
    {
        public LibraryLoaderCDImportView()
        {
            InitializeComponent();
        }
    }
}
