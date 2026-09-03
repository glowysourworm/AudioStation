using System.IO;

using AudioStation.Component.Interface;
using AudioStation.Core;
using AudioStation.Core.Component;
using AudioStation.Core.Utility;
using AudioStation.Event;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component
{
    [IocExport(typeof(IAudioStationConfigurationManager))]
    public class AudioStationConfigurationManager : IAudioStationConfigurationManager
    {
        private const string CONFIGURATION_FILE = ".AudioStation";

        AudioStationConfiguration _configuration;

        public event SimpleEventHandler<AudioStationConfiguration, ConfigurationEventType, bool> ConfigurationEvent;

        [IocImportingConstructor]
        public AudioStationConfigurationManager()
        {
        }

        public void Initialize(string? configurationFile)
        {
            // Current working directory + configuration file name
            var configFileName = string.IsNullOrWhiteSpace(configurationFile) ? CONFIGURATION_FILE : configurationFile;

            _configuration = this.Open(configFileName);

            if (this.ConfigurationEvent != null)
                this.ConfigurationEvent(_configuration, ConfigurationEventType.Open, ValidateConfiguration());
        }

        public AudioStationConfiguration GetConfiguration()
        {
            if (_configuration == null)
                throw new Exception("Configuration not properly initialized. Cannot get a valid configuration. Please make sure to properly set configuration path.");

            return _configuration;
        }

        public AudioStationConfiguration GetValidConfiguration()
        {
            if (!ValidateConfiguration())
                throw new Exception("Configuration not valid! Cannot return valid configuration. Please check before using this method!");

            return _configuration;
        }

        public void SaveConfiguration()
        {
            Save();
        }

        public void SaveConfiguration(AudioStationConfiguration configuration)
        {
            _configuration = configuration;

            if (this.ConfigurationEvent != null)
                this.ConfigurationEvent(_configuration, ConfigurationEventType.Modified, ValidateConfiguration());

            Save();
        }

        private void Save()
        {
            try
            {
                // Command Line -> Default Name
                var configPath = ResolveConfigurationFile();

                // Configuration
                Serializer.Serialize(_configuration, configPath);

                ApplicationHelpers.Log("Configuration saved successfully: {0}", LogLevel.Information, null, configPath);

                if (this.ConfigurationEvent != null)
                    this.ConfigurationEvent(_configuration, ConfigurationEventType.Saved, ValidateConfiguration());
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error saving configuration / data files:  {0}", LogLevel.Error, ex, ex.Message);
            }
        }
        private AudioStationConfiguration Open(string configurationFile)
        {
            try
            {
                return (AudioStationConfiguration)Serializer.Deserialize<AudioStationConfiguration>(configurationFile);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error reading configuration file. Please try saving the working configuration first and then restarting.", LogLevel.Error, LogMessageType.General, ex);
                ApplicationHelpers.Log("Creating default configuration.");

                return new AudioStationConfiguration();
            }
        }

        private string ResolveConfigurationFile()
        {
            // Current working directory + configuration file name
            var configPath = string.Empty;

            if (Environment.GetCommandLineArgs().Length > 1)
                configPath = Environment.GetCommandLineArgs()[1];
            else
                configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIGURATION_FILE);

            return configPath;
        }

        public bool ValidateConfiguration()
        {
            if (_configuration == null)
                return false;

            try
            {
                var success = true;

                success &= !string.IsNullOrWhiteSpace(_configuration.AcoustIDAPIKey);
                success &= !string.IsNullOrWhiteSpace(_configuration.ApplicationCacheFolder.Directory);
                success &= !string.IsNullOrWhiteSpace(_configuration.ApplicationStorageFolder.Directory);
                success &= !string.IsNullOrWhiteSpace(_configuration.BandcampAPIKey);
                success &= !string.IsNullOrWhiteSpace(_configuration.BandcampAPISecret);
                success &= !string.IsNullOrWhiteSpace(_configuration.BandcampEmail);
                success &= !string.IsNullOrWhiteSpace(_configuration.BandcampPassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.DatabaseHost);
                success &= !string.IsNullOrWhiteSpace(_configuration.DatabaseName);
                success &= !string.IsNullOrWhiteSpace(_configuration.DatabasePassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.DatabaseUser);
                success &= !string.IsNullOrWhiteSpace(_configuration.DiscogsCurrentToken);
                success &= !string.IsNullOrWhiteSpace(_configuration.DiscogsEmail);
                success &= !string.IsNullOrWhiteSpace(_configuration.DiscogsKey);
                success &= !string.IsNullOrWhiteSpace(_configuration.DiscogsSecret);
                success &= !string.IsNullOrWhiteSpace(_configuration.DownloadFolder.Directory);
                success &= !string.IsNullOrWhiteSpace(_configuration.FanartAPIKey);
                success &= !string.IsNullOrWhiteSpace(_configuration.FanartEmail);
                success &= !string.IsNullOrWhiteSpace(_configuration.FanartPassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.FanartUser);
                success &= !string.IsNullOrWhiteSpace(_configuration.LastFmAPIKey);
                success &= !string.IsNullOrWhiteSpace(_configuration.LastFmAPISecret);
                success &= !string.IsNullOrWhiteSpace(_configuration.LastFmAPIUser);
                success &= !string.IsNullOrWhiteSpace(_configuration.LastFmApplication);
                success &= !string.IsNullOrWhiteSpace(_configuration.LastFmPassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.LastFmUser);
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicBrainzPassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicBrainzUser);
                success &= !string.IsNullOrWhiteSpace(_configuration.SpotifyClientId);
                success &= !string.IsNullOrWhiteSpace(_configuration.SpotifyClientSecret);
                success &= !string.IsNullOrWhiteSpace(_configuration.StagingFolder.Directory);

                success &= !_configuration.LibraryDirectories.Any(x => string.IsNullOrWhiteSpace(x.Directory));
                success &= !_configuration.LibraryDirectories.Any(x => !Directory.Exists(x.Directory));

                if (!Directory.Exists(_configuration.ApplicationCacheFolder.Directory))
                    success = false;

                if (!Directory.Exists(_configuration.ApplicationStorageFolder.Directory))
                    success = false;

                if (!Directory.Exists(_configuration.StagingFolder.Directory))
                    success = false;

                if (!Directory.Exists(_configuration.DownloadFolder.Directory))
                    success = false;

                return success;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error validating configuration:  {0}", LogMessageComponentType.ConfigurationManager, LogLevel.Error, ex, ex.Message);
                return false;
            }
        }
    }
}
