using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component
{
    [IocExport(typeof(IConfigurationManager))]
    public class ConfigurationManager : IConfigurationManager
    {
        private const string CONFIGURATION_FILE = ".AudioStation";
        private const string RADIO_FILE = ".AudioStationRadio";

        private readonly IOutputController _outputController;

        AudioStationConfiguration _configuration;

        [IocImportingConstructor]
        public ConfigurationManager(IOutputController outputController)
        {
            _outputController = outputController;
        }

        public void Initialize(string? configurationFile)
        {
            // Current working directory + configuration file name
            var configFileName = configurationFile ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIGURATION_FILE);

            _configuration = this.Open(configFileName);
        }

        public AudioStationConfiguration GetConfiguration()
        {
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

        private void Save()
        {
            try
            {
                // Command Line -> Default Name
                var configPath = ResolveConfigurationFile();

                // Configuration
                Serializer.Serialize(_configuration, configPath);

                _outputController.Log("Configuration saved successfully: {0}", LogMessageType.General, configPath);
            }
            catch (Exception ex)
            {
                _outputController.Log("Error saving configuration / data files:  {0}", LogLevel.Error, LogMessageType.General, ex, ex.Message);
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
                _outputController.Log("Error reading configuration file. Please try saving the working configuration first and then restarting.", LogLevel.Error, LogMessageType.General, ex);
                _outputController.Log("Creating default configuration.");

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
