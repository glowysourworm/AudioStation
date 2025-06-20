using AudioStation.ViewModels.Interface;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class StartPlaybackEvent : IocEvent<INowPlayingViewModel>
    {
    }
}
