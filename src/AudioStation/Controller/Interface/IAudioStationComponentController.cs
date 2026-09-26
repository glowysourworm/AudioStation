using AudioStation.Core;
using AudioStation.ViewModels;

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
        T GetServiceComponent<T>() where T : ServiceComponentViewModelBase;

        /// <summary>
        /// Returns a component from the application's view model tree
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        T GetDataComponent<T>() where T : DataComponentViewModelBase;

        /// <summary>
        /// Loads or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        void LoadComponent<T>(bool showProgress) where T : ServiceComponentViewModelBase;

        /// <summary>
        /// Asynchronously loads or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        Task LoadComponentAsync<T>() where T : ServiceComponentViewModelBase;

        /// <summary>
        /// Executes or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        void ExecuteComponent<T>(bool showProgress) where T : ServiceComponentViewModelBase;

        /// <summary>
        /// Asynchronously executes or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        Task ExecuteComponentAsync<T>() where T : ServiceComponentViewModelBase;

        /// <summary>
        /// Resets or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        void ResetComponent<T>(bool showProgress) where T : ServiceComponentViewModelBase;

        /// <summary>
        /// Asynchronously resets or re-initializes a component to prepare for work load
        /// </summary>
        /// <typeparam name="T">Component type</typeparam>
        Task ResetComponentAsync<T>() where T : ServiceComponentViewModelBase;
    }
}
