using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using AudioStation.Event;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class PlayerControlView : UserControl
    {
        IIocEventAggregator _eventAggregator;

        [IocImportingConstructor]
        public PlayerControlView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();

            // There are problems binding observable collections to dependency properties. So, these
            // have to be set manually.
            _eventAggregator.GetEvent<PlaybackEqualizerUpdateEvent>().Subscribe(equalizerValues =>
            {
                this.EqualizerView.SetEqualizer(equalizerValues);
            });
        }

        private void VolumeButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.VolumePopup.IsOpen == true)
            {
                this.VolumeButton.IsChecked = false;
                this.VolumePopup.IsOpen = false;
            }
            else
            {
                this.VolumeButton.IsChecked = true;
                this.VolumePopup.IsOpen = true;
            }
        }

        private void VolumePopup_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var result = VisualTreeHelper.HitTest(this.VolumeControlContainer, e.GetPosition(this.VolumeControlContainer));

            if (result == null)
            {
                this.VolumeButton.IsChecked = false;
                this.VolumePopup.IsOpen = false;
            }
        }

        private void VolumeControl_ScrubbedRatioChanged(float volume)
        {
            _eventAggregator.GetEvent<UpdateVolumeEvent>().Publish(volume);
        }

        private void ScrubberControl_ScrubbedRatioChanged(float sender)
        {
            var playlist = this.DataContext as PlaylistViewModel;

            if (playlist != null)
            {
                _eventAggregator.GetEvent<PlaybackPositionChangedEvent>()
                                .Publish(TimeSpan.FromMilliseconds(sender * playlist.NowPlaying.Track.Duration.TotalMilliseconds));
            }
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void PauseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<PausePlaybackEvent>().Publish();
        }

        private void PlayButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<StartPlaybackEvent>().Publish();
        }

        private void StopButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<StopPlaybackEvent>().Publish();
        }

        private void ForwardButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
