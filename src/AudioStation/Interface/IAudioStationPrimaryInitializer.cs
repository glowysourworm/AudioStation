using AudioStation.Core;
using AudioStation.EventHandler;

namespace AudioStation.Interface
{
    /// <summary>
    /// Primary component for the audio station - involved in the main life cycle of the view, view model, 
    /// service, and configuration components.
    /// </summary>
    public interface IAudioStationPrimaryInitializer
    {
        void Initialize(AudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler);
    }
}
