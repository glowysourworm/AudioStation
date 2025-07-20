using System.IO;

using AudioStation.Core.Component.Interface;
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

        Configuration _configuration;

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

        public Configuration GetConfiguration()
        {
            return _configuration;
        }

        public Configuration GetValidConfiguration()
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

                _outputController.Log("Configuration saved successfully: {0}", LogMessageType.General, LogLevel.Information, configPath);
            }
            catch (Exception ex)
            {
                _outputController.Log("Error saving configuration / data files:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }
        }
        private Configuration Open(string configurationFile)
        {
            try
            {
                return (Configuration)Serializer.Deserialize<Configuration>(configurationFile);
            }
            catch (Exception ex)
            {
                _outputController.Log(new LogMessage("Error reading configuration file. Please try saving the working configuration first and then restarting.", LogMessageType.General, LogLevel.Error));
                _outputController.Log(new LogMessage("Creating default configuration.", LogMessageType.General, LogLevel.Error));

                return new Configuration();
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
                success &= !string.IsNullOrWhiteSpace(_configuration.AudioBooksSubDirectory);
                success &= !string.IsNullOrWhiteSpace(_configuration.BandcampAPIKey);
                success &= !string.IsNullOrWhiteSpace(_configuration.BandcampAPISecret);
                success &= !string.IsNullOrWhiteSpace(_configuration.BandcampEmail);
                success &= !string.IsNullOrWhiteSpace(_configuration.BandcampPassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.DatabaseHost);
                success &= !string.IsNullOrWhiteSpace(_configuration.DatabaseName);
                success &= !string.IsNullOrWhiteSpace(_configuration.DatabasePassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.DatabaseUser);
                success &= !string.IsNullOrWhiteSpace(_configuration.DirectoryBase);
                success &= !string.IsNullOrWhiteSpace(_configuration.DiscogsCurrentToken);
                success &= !string.IsNullOrWhiteSpace(_configuration.DiscogsEmail);
                success &= !string.IsNullOrWhiteSpace(_configuration.DiscogsKey);
                success &= !string.IsNullOrWhiteSpace(_configuration.DiscogsSecret);
                success &= !string.IsNullOrWhiteSpace(_configuration.DownloadFolder);
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
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicBrainzDatabaseHost);
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicBrainzDatabaseName);
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicBrainzDatabasePassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicBrainzDatabaseUser);
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicBrainzPassword);
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicBrainzUser);
                success &= !string.IsNullOrWhiteSpace(_configuration.MusicSubDirectory);
                success &= !string.IsNullOrWhiteSpace(_configuration.SpotifyClientId);
                success &= !string.IsNullOrWhiteSpace(_configuration.SpotifyClientSecret);

                if (!Directory.Exists(_configuration.DirectoryBase))
                    success = false;

                if (!Directory.Exists(Path.Combine(_configuration.DirectoryBase, _configuration.MusicSubDirectory)))
                    success = false;

                if (!Directory.Exists(Path.Combine(_configuration.DirectoryBase, _configuration.AudioBooksSubDirectory)))
                    success = false;

                if (!Directory.Exists(_configuration.DownloadFolder))
                    success = false;

                return success;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error validating configuration:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                return false;
            }
        }
    }
}
