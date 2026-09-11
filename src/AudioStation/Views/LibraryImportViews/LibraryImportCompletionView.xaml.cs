using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryImportViews
{
    [IocExportDefault]
    public partial class LibraryImportCompletionView : UserControl
    {
        public LibraryImportCompletionView()
        {
            InitializeComponent();
        }
    }
}
