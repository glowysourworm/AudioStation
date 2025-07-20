using SimpleWpf.Extensions;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels
{
    /// <summary>
    /// View model base for a "primary" view model - which contains major pieces of the
    /// application's data. So, there is a life cycle pattern for handling the data from
    /// a controller.
    /// </summary>
    public abstract class PrimaryViewModelBase : ViewModelBase, IDisposable
    {
        public abstract Task Initialize(DialogProgressHandler progressHandler);
        public abstract void Dispose();
    }
}
