using AudioStation.Component.Model;
using AudioStation.ViewModels;
using AudioStation.ViewModels.PlaylistViewModels.Interface;

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
