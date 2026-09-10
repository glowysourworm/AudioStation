using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Event;

namespace AudioStation.Service.Interface
{
    public interface IAudioStationService
    {
        void Initialize(AudioStationConfiguration configuration, IAudioStationController audioStationController, DialogEventHandlers.DialogProgressHandler progressHandler);
    }
}
