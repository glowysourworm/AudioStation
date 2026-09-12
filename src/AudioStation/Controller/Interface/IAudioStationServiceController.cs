using AudioStation.Core;
using AudioStation.Core.Service.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controller.Interface
{
    /// <summary>
    /// Component controller to report and contain all IAudioStationComponent instances
    /// </summary>
    public interface IAudioStationServiceController
    {
        void Initialize(AudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler);

        /// <summary>
        /// Occurs when component is initialized
        /// </summary>
        event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> ComponentInitializedEvent;

        /// <summary>
        /// Occurs when component status changes
        /// </summary>
        event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> ComponentStatusChangedEvent;

        /// <summary>
        /// Returns data service component based on interface type
        /// </summary>
        T GetDataService<T>() where T : IAudioStationDataService;

        /// <summary>
        /// Returns service component based on interface type
        /// </summary>
        T GetService<T>() where T : IAudioStationService;

        /// <summary>
        /// Returns a cache based on the interface type
        /// </summary>
        T GetCache<T>() where T : IAudioStationCache;
    }
}
