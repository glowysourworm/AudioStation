using AudioStation.Core;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public enum ConfigurationEventType
    {
        Opened = 0,
        Modified = 1,
        Saved = 2,
        SaveRequest = 3,
    }

    public class ConfigurationEventData
    {
        public ConfigurationEventType Type { get; set; }
        public bool IsConfigurationValid { get; set; }
        public AudioStationConfiguration? Configuration { get; set; }
        public AudioStationConfigurationViewModel? ViewModel { get; set; }

        public ConfigurationEventData()
        {
        }
    }

    public class ConfigurationEvent : IocEvent<ConfigurationEventData>
    {
    }
}
