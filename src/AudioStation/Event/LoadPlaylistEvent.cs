using AudioStation.ViewModels.PlaylistViewModels.Interface;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class LoadPlaylistEventData
    {
        public IEnumerable<IPlaylistEntryViewModel> PlaylistEntries { get; set; }
        public IPlaylistEntryViewModel StartTrack { get; set; }
    }
    public class LoadPlaylistEvent : IocEvent<LoadPlaylistEventData>
    {
    }
}
