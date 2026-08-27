using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views.LibraryLoaderViews
{
    [IocExportDefault]
    public partial class LibraryLoaderMusicBrainzBasicView : UserControl
    {
        [IocImportingConstructor]
        public LibraryLoaderMusicBrainzBasicView()
        {
            InitializeComponent();
        }
    }
}
