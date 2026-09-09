using AudioStation.Core;
using AudioStation.Interface;
using AudioStation.Service.Interface;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.Controller.Interface
{
    /// <summary>
    /// Primary component controller. Forwards configuration events. Contains other component and service
    /// controllers.
    /// </summary>
    public interface IAudioStationController : IAudioStationPrimaryInitializer
    {
        IAudioStationConfigurationController ConfigurationController { get; }
        IAudioStationServiceController ServiceController { get; }
        IAudioStationComponentController ComponentController { get; }
        ILibraryLoaderService LibraryLoaderService { get; }

        /// <summary>
        /// (Primary Initializer!) Startup must handle configuration first. Then call Initialize(...)
        /// </summary>
        AudioStationConfiguration InitializeConfiguration(string configurationFile, DialogProgressHandler progressHandler);
    }
}
