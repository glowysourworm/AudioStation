using AudioStation.EventHandler;
using AudioStation.ViewModels;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Controller.Interface
{
    /// <summary>
    /// Controller responsible for the life cycle of the view (main) models in the application. This would
    /// include the MainViewModel; and each of its public view model properties (the other "main view models")
    /// that are data contexts for the primary components of the application. Each tab should have its own
    /// view model; and also the log component.
    /// </summary>
    public interface IViewModelController
    {
        /// <summary>
        /// Loads the initial state of the view models for the application. This should be run prior to showing the main window.
        /// </summary>
        Task Initialize(DialogProgressHandler progressHandler);

        /// <summary>
        /// Returns the (loaded) main view model instance for the application
        /// </summary>
        MainViewModel GetMainViewModel();
    }
}
