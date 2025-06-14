using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryManager
{
    [IocExportDefault]
    public partial class ManagerView : UserControl
    {
        [IocImportingConstructor]
        public ManagerView()
        {
            InitializeComponent();
        }
    }
}
