using System.Windows.Controls;

using AudioStation.Controller.Interface;
using AudioStation.ViewModels.ComponentViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class LibraryLoaderView : UserControl
    {
        [IocImportingConstructor]
        public LibraryLoaderView(IAudioStationController audioStationController)
        {
            InitializeComponent();

            this.DataContext = audioStationController.ComponentController.GetComponent<LibraryLoaderViewModel>();
        }
    }
}
