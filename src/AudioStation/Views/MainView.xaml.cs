using System.Windows.Controls;

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

        private void RadioBrowserView_StartStationEvent(ViewModels.RadioViewModel.RadioStationViewModel sender)
        {
            var nowPlaying = new NowPlayingViewModel()
            {
                Title = sender.Name,
                Source = sender.Endpoint,
                SourceType = Model.StreamSourceType.Network
            };

            MainViewModel.AudioController.Play(nowPlaying);
        }
    }
}
