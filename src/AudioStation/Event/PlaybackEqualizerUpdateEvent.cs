using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class PlaybackEqualizerUpdateEvent : IocEvent<float[]>
    {
    }
}
