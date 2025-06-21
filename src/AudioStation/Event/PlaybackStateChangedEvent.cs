using AudioStation.Core.Component;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class PlaybackStateChangedEvent : IocEvent<PlayStopPause>
    {
    }
}
