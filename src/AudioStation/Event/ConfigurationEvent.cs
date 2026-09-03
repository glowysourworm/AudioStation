using AudioStation.Core;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Event
{
    public enum ConfigurationEventType
    {
        Open = 0,
        Modified = 1,
        Saved = 2
    }

    public class ConfigurationEventData
    {
        public ConfigurationEventType Type { get; set; }
        public bool IsConfigurationValid { get; set; }
        public AudioStationConfiguration Configuration { get; private set; }
        public AudioStationConfigurationViewModel ViewModel { get; private set; }

        public ConfigurationEventData(AudioStationConfiguration configuration, AudioStationConfigurationViewModel viewModel)
        {
            this.Configuration = configuration;
            this.ViewModel = viewModel;
        }
    }

    public class ConfigurationEvent : IocEvent<ConfigurationEventData>
    {
    }
}
