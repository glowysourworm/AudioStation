using System.Windows.Controls;

using AudioStation.Core.Model;
using AudioStation.ViewModels;

namespace AudioStation.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void OnShowOutputMessagesClick(object sender, System.Windows.RoutedEventArgs e)
        {
            (this.DataContext as MainViewModel).ShowOutputMessages = !(this.DataContext as MainViewModel).ShowOutputMessages;
        }

        private void RadioBrowserView_StartStationEvent(ViewModels.RadioViewModels.RadioStationViewModel sender)
        {
            //var nowPlaying = new NowPlayingViewModel()
            //{
            //    Bitrate = sender.Bitrate,
            //    Codec = sender.Codec,
            //    Title = sender.Name,
            //    Source = sender.Endpoint,
            //    SourceType = StreamSourceType.Network
            //};

            //MainViewModel.AudioController.Play(nowPlaying);
        }
    }
}
