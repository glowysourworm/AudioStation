using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using AudioStation.Controls;
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
                this.EqualizerOutputControl.SetEqualizer(equalizerValues);
            });
        }

        private void SetAudioControls()
        {
            AudioControlPanelControl controls = AudioControlPanelControl.None;

            if (this.VolumeButton.IsChecked == true)
                controls |= AudioControlPanelControl.Volume;

            if (this.EqualizerButton.IsChecked == true)
                controls |= AudioControlPanelControl.Equalizer;

            _eventAggregator.GetEvent<NowPlayingShowAudioControlPanelEvent>()
                            .Publish(controls);
        }

        private void ScrubberControl_ScrubbedRatioChanged(ScrubberControl sender, float current)
        {
            var playlist = this.DataContext as PlaylistViewModel;

            if (playlist != null && playlist.CurrentTrack != null)
            {
                _eventAggregator.GetEvent<PlaybackPositionChangedEvent>()
                                .Publish(TimeSpan.FromMilliseconds(current * playlist.CurrentTrack.Track.Duration.TotalMilliseconds));
            }
        }

        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _eventAggregator.GetEvent<LoadPreviousTrackEvent>().Publish();
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
            _eventAggregator.GetEvent<LoadNextTrackEvent>().Publish();
        }

        private void ExpandedViewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ListViewButton.IsChecked = false;

            _eventAggregator.GetEvent<NowPlayingExpandedViewEvent>().Publish(true);
        }

        private void ListViewButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ExpandedViewButton.IsChecked = false;

            _eventAggregator.GetEvent<NowPlayingExpandedViewEvent>().Publish(false);
        }

        private void VolumeButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            SetAudioControls();
        }

        private void EqualizerButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            SetAudioControls();
        }
    }
}
