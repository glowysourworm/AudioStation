using AudioStation.Core.Component.CDPlayer;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class CDPlayerLoadEvent : IocEvent<CDDeviceTracksLoadedEventArgs>
    {
    }
}
