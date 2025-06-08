using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Core.Event
{
    // There could be several of these stored around:  one per log type
    public class LogConfigurationData
    {
        public bool Verbose { get; set; }
        public LogLevel Level { get; set; }
        public LogMessageType Type { get; set; }
    }

    public class LogConfigurationChangedEvent : IocEvent<LogConfigurationData>
    {
    }
}
