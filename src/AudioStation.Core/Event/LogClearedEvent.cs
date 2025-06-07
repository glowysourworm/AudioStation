using AudioStation.Model;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Core.Event
{
    public class LogClearedEvent : IocEvent<LogMessageType>
    {
    }
}
