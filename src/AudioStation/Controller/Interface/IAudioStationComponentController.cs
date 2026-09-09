using AudioStation.Core;
using AudioStation.ViewModels.ComponentViewModels;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.Controller.Interface
{
    public interface IAudioStationComponentController
    {
        void Initialize(AudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);

        /// <summary>
        /// Returns a component from the application's view model tree
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        T GetComponent<T>() where T : ComponentViewModelBase;

        /// <summary>
        /// Loads or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        void LoadComponent<T>() where T : ComponentViewModelBase;

        /// <summary>
        /// Asynchronously loads or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        Task LoadComponentAsync<T>() where T : ComponentViewModelBase;
    }
}
