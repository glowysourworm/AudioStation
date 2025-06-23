using System.Windows.Controls;

using AudioStation.Controls;
using AudioStation.Event;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Views
{
    [IocExportDefault]
    public partial class PlayerEqualizerView : UserControl
    {
        IIocEventAggregator _eventAggregator;

        [IocImportingConstructor]
        public PlayerEqualizerView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();
        }

        private void ScrubberControl_ScrubbedRatioChanged(ScrubberControl sender, float gain)
        {
            _eventAggregator.GetEvent<UpdateEqualizerGainEvent>().Publish(new UpdateEqualizerGainEventData()
            {
                Gain = gain,
                Frequency = (float)sender.Tag
            });
        }
    }
}
