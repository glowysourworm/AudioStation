using AudioStation.Core;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    /// <summary>
    /// Configuration events that happen in the View + ViewModel layers of the application
    /// </summary>
    public enum ConfigurationEventType
    {
        /// <summary>
        /// Configuration (source) is opened and broadcast
        /// </summary>
        Opened = 0,

        /// <summary>
        /// Configuration (source) is modified and broadcast
        /// </summary>
        Modified = 1,

        /// <summary>
        /// Configuration (target) is modified and a request is sent to modify the (source)
        /// </summary>
        ModifyRequest = 2,

        /// <summary>
        /// Configuration (source) is saved
        /// </summary>
        Saved = 3,

        /// <summary>
        /// Configuration (target) requests a save of the configuration
        /// </summary>
        SaveRequest = 4,
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
