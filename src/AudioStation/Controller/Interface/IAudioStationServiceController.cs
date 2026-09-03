using AudioStation.Core.Service.Interface;
using AudioStation.Interface;

using SimpleWpf.Extensions.Event;

namespace AudioStation.Controller.Interface
{
    /// <summary>
    /// Component controller to report and contain all IAudioStationComponent instances
    /// </summary>
    public interface IAudioStationServiceController : IAudioStationPrimaryInitializer
    {
        /// <summary>
        /// Occurs when component is initialized
        /// </summary>
        event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> ComponentInitializedEvent;

        /// <summary>
        /// Occurs when component status changes
        /// </summary>
        event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> ComponentStatusChangedEvent;

        /// <summary>
        /// Returns componet based on (interface) type
        /// </summary>
        T GetComponent<T>() where T : IAudioStationService;
    }
}
