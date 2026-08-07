using AudioStation.Core.Service.Interface;

using SimpleWpf.Extensions.Event;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Controller.Interface
{
    /// <summary>
    /// Component controller to report and contain all IAudioStationComponent instances
    /// </summary>
    public interface IAudioStationComponentController
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
        /// Runs primary initialization routines for each IAudioStationComponent. This should be run prior to showing the main window.
        /// </summary>
        Task Initialize(DialogProgressHandler progressHandler);

        /// <summary>
        /// Returns componet based on (interface) type
        /// </summary>
        T GetComponent<T>() where T : IAudioStationService;
    }
}
