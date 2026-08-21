using AudioStation.Component.Model;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class LoadPlaylistEventData
    {
        public NowPlayingData NowPlayingData { get; set; }
        public bool StartPlayback { get; set; }
    }
    public class LoadPlaylistEvent : IocEvent<LoadPlaylistEventData>
    {
    }
}
