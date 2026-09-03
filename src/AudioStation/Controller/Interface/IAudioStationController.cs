using AudioStation.Core;
using AudioStation.Interface;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Controller.Interface
{
    /// <summary>
    /// Primary component controller. Forwards configuration events. Contains other component and service
    /// controllers.
    /// </summary>
    public interface IAudioStationController : IAudioStationPrimaryInitializer
    {
        /// <summary>
        /// (Primary Initializer!) Startup must handle configuration first. Then call Initialize(...)
        /// </summary>
        AudioStationConfiguration InitializeConfiguration(string configurationFile, DialogProgressHandler progressHandler);
    }
}
