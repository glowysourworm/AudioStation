using System.ComponentModel;
using System.IO;

using AudioStation.Core.Component.Interface;
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

                _outputController.AddLog("Configuration saved successfully: {0}", LogMessageType.General, LogLevel.Information, configPath);
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error saving configuration / data files:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
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
                _outputController.AddLog(new LogMessage("Error reading configuration file. Please try saving the working configuration first and then restarting.", LogMessageType.General, LogLevel.Error));
                _outputController.AddLog(new LogMessage("Creating default configuration.", LogMessageType.General, LogLevel.Error));

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
    }
}
