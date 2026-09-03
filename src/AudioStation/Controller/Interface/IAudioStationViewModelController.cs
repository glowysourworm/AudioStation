using AudioStation.Interface;
using AudioStation.ViewModels.ComponentViewModels;

namespace AudioStation.Controller.Interface
{
    public interface IAudioStationViewModelController : IAudioStationPrimaryInitializer
    {
        T GetComponent<T>() where T : ComponentViewModelBase;
    }
}
