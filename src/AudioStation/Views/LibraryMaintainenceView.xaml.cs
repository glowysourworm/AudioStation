using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibraryMaintainenceView : UserControl
    {
        public LibraryMaintainenceView()
        {
            InitializeComponent();
        }
    }
}
