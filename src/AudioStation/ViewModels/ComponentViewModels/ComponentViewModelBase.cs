using AudioStation.Core;

using SimpleWpf.ViewModel;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.ComponentViewModels
{
    /// <summary>
    /// View model base for a "primary" view model - which contains major pieces of the
    /// application's data. So, there is a life cycle pattern for handling the data from
    /// a controller. The "Load" data type will be used to send data to the view model.
    /// </summary>
    public abstract class ComponentViewModelBase<TLoad> : ViewModelBase, IDisposable where TLoad : ViewModelBase
    {
        /// <summary>
        /// Returns the primary load of the view model
        /// </summary>
        public abstract TLoad? Load { get; }

        public abstract void Initialize(Configuration configuration, TLoad load, DialogProgressHandler progressHandler);
        public abstract void Dispose();
    }
}
