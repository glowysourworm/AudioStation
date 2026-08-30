using SimpleWpf.Extensions.Event;

namespace AudioStation.Core.Service.Interface
{
    /// <summary>
    /// Primary component of Audio Station core. Each of these components will be displayed to the user
    /// on the status bar as "working", or "not working". This interface will help them to know whether
    /// each piece has had some malfunction; and this interface will be called during startup to gather
    /// component status and initialize the service.
    /// </summary>
    public interface IAudioStationService
    {
        public enum Status
        {
            Disabled = 0,
            Idle = 1,
            Working = 2,
            Error = 3
        }

        static string GetDefaultStatusMessage(Status status)
        {
            switch (status)
            {
                case Status.Disabled:
                case Status.Idle:
                case Status.Working:
                case Status.Error:
                    return status.ToString();
                default:
                    throw new Exception("Unhandled IAudioStationComponent.Status type");
            }
        }

        /// <summary>
        /// Gets name of component
        /// </summary>
        string GetName();

        /// <summary>
        /// Gets display name of component
        /// </summary>
        string GetDisplayName();

        /// <summary>
        /// Signals a status change event
        /// </summary>
        event SimpleEventHandler<IAudioStationService, Status> StatusChangeEvent;

        /// <summary>
        /// Returns current status of component
        /// </summary>
        Status GetStatus();

        /// <summary>
        /// Performs startup task for component
        /// </summary>
        Status Initialize(AudioStationConfiguration configuration);

        /// <summary>
        /// Performs startup task for component
        /// </summary>
        Task<Status> InitializeAsync(AudioStationConfiguration configuration);

        /// <summary>
        /// Resets the component based on new configuration settings
        /// </summary>
        Status ReInitialize(AudioStationConfiguration configuration);

        /// <summary>
        /// Resets the component based on new configuration settings
        /// </summary>
        Task<Status> ReInitializeAsync(AudioStationConfiguration configuration);

        /// <summary>
        /// Returns status message for the component
        /// </summary>
        string GetStatusMessage();
    }
}
