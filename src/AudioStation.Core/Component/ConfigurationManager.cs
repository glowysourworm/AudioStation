using System.ComponentModel;
using System.IO;

using AudioStation.Core.Component.Interface;
using AudioStation.Model;

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

        public void Initialize()
        {
            _configuration = this.Open();

            _configuration.PropertyChanged += OnConfigurationChanged;
        }

        public Configuration GetConfiguration()
        {
            return _configuration;
        }

        private void Save()
        {
            try
            {
                // Command Line -> Default Name
                var configPath = ResolveConfigurationFile();

                // Configuration
                Serializer.Serialize(_configuration, configPath);

                _outputController.AddLog("Configuration saved successfully: {0}", LogMessageType.General, LogMessageSeverity.Info, configPath);
            }
            catch (Exception ex)
            {
                _outputController.AddLog("Error saving configuration / data files:  {0}", LogMessageType.General, LogMessageSeverity.Error, ex.Message);
            }
        }
        private Configuration Open()
        {
            try
            {
                // Current working directory + configuration file name
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIGURATION_FILE);

                return (Configuration)Serializer.Deserialize<Configuration>(configPath);
            }
            catch (Exception ex)
            {
                _outputController.AddLog(new LogMessage("Error reading configuration file. Please try saving the working configuration first and then restarting.", LogMessageSeverity.Error));
                _outputController.AddLog(new LogMessage("Creating default configuration.", LogMessageSeverity.Error));

                return new Configuration()
                {
                    DatabaseHost = "localhost",
                    DatabaseName = "AudioStation",
                    DatabaseUser = "postgres",
                    DatabasePassword = "!ngndol234",
                    DirectoryBase = "C:\\Backup\\audio-library"
                };
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

        private void OnConfigurationChanged(object? sender, PropertyChangedEventArgs e)
        {
            Save();
        }
    }
}
