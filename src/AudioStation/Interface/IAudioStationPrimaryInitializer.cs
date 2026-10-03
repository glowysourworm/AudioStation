using AudioStation.Core;
using AudioStation.Event;

namespace AudioStation.Interface
{
    /// <summary>
    /// Primary component for the audio station - involved in the main life cycle of the view, view model, 
    /// service, and configuration components.
    /// </summary>
    public interface IAudioStationPrimaryInitializer
    {
        bool Initialized { get; }

        /// <summary>
        /// Method run on initialization of the application
        /// </summary>
        void Initialize(AudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler);

        /// <summary>
        /// Method run during application shutdown
        /// </summary>
        void Shutdown();
    }
}
