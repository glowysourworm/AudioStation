using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class PlaybackPositionChangedEvent : IocEvent<TimeSpan>
    {
    }
}
