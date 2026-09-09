using AudioStation.Model.AudioProcessing;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class PlaybackEqualizerUpdateEvent : IocEvent<EqualizerResultSet>
    {
    }
}
