using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model;
using AudioStation.ViewModels;
using AudioStation.ViewModels.RadioViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class RadioBrowserView : UserControl
    {
        private readonly IAudioController _audioController;

        public RadioBrowserView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public RadioBrowserView(IAudioController audioController)
        {
            _audioController = audioController;

            InitializeComponent();
        }

        private void RadioStationLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (e.OriginalSource as FrameworkElement).DataContext as RadioStationViewModel;

            if (viewModel != null)
            {
                _audioController.Play(new NowPlayingViewModel()
                {
                    Bitrate = viewModel.Bitrate,
                    Codec = viewModel.Codec,
                    Source = viewModel.Endpoint,
                    SourceType = StreamSourceType.Network,
                    Title = viewModel.Name,
                });
            }
        }
    }
}
