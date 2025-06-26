using AudioStation.Core.Component;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class PlaybackStateChangedEventData
    {
        public PlayStopPause State { get; set; }
        public bool UserInitiated { get; set; }
    }

    public class PlaybackStateChangedEvent : IocEvent<PlaybackStateChangedEventData>
    {
    }
}
