using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryLoaderViews
{
    [IocExportDefault]
    public partial class LibraryLoaderMusicBrainzView : UserControl
    {
        [IocImportingConstructor]
        public LibraryLoaderMusicBrainzView()
        {
            InitializeComponent();
        }
    }
}
