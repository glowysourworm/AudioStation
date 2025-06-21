using AudioStation.Core.Model;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class LoadPlaybackEventData
    {
        public string Source { get; set; }
        public StreamSourceType SourceType { get; set; }
    }
    public class LoadPlaybackEvent : IocEvent<LoadPlaybackEventData>
    {
    }
}
