using AudioStation.Core.Component.CDPlayer;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public class CDPlayerReadEvent : IocEvent<CDDataReadEventArgs>
    {
    }
}
