using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService
    {
        /// <summary>
        /// Initialization of the view model - this should be run during startup
        /// </summary>
        void Initialize(DialogProgressHandler progressHandler);
    }
}
