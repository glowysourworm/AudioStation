﻿namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Component responsible for managing the application's configuration (object!). After
    /// property change, the manager will save the configuration to file.
    /// </summary>
    public interface IConfigurationManager
    {
        void Initialize(string? configurationFile);
        Configuration GetValidConfiguration();
        Configuration GetConfiguration();
        void SaveConfiguration();
        bool ValidateConfiguration();
    }
}
