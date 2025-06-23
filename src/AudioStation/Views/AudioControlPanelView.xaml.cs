using System.Windows;
using System.Windows.Controls;

using AudioStation.Controls;
using AudioStation.Event;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class AudioControlPanelView : UserControl
    {
        private readonly IIocEventAggregator _eventAggregator;

        [IocImportingConstructor]
        public AudioControlPanelView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();

            eventAggregator.GetEvent<NowPlayingShowAudioControlPanelEvent>().Subscribe(controls =>
            {
                this.VolumeControlContainer.Visibility = Visibility.Collapsed;
                this.EqualizerControlContainer.Visibility = Visibility.Collapsed;

                if (controls.HasFlag(AudioControlPanelControl.Volume))
                    this.VolumeControlContainer.Visibility = Visibility.Visible;

                if (controls.HasFlag(AudioControlPanelControl.Equalizer))
                    this.EqualizerControlContainer.Visibility = Visibility.Visible;
            });
        }

        private void VolumeControl_ScrubbedRatioChanged(ScrubberControl sender, float volume)
        {
            _eventAggregator.GetEvent<UpdateVolumeEvent>().Publish(volume);
        }
    }
}
