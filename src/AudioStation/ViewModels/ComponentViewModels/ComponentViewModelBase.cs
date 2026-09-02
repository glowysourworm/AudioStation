using System.Windows.Threading;

using AudioStation.Core.Model.Interface;

using SimpleWpf.Utilities;
using SimpleWpf.UI.ViewModel;

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
        bool _loading;

        /// <summary>
        /// (TODO: Controller pattern!!!) Component is currently running an operation
        /// </summary>
        public bool Loading
        {
            get { return _loading; }
            set { this.RaiseAndSetIfChanged(ref _loading, value); }
        }

        /// <summary>
        /// Returns the primary load of the view model
        /// </summary>
        public abstract TLoad? Load { get; }

        /// <summary>
        /// Function to complete initialization. This will be called on the Dispatcher thread
        /// </summary>
        protected abstract void InitializeWork(IAudioStationConfiguration configuration, TLoad load, DialogProgressHandler progressHandler);

        public void Initialize(IAudioStationConfiguration configuration, TLoad load, DialogProgressHandler progressHandler)
        {
            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Initialize, DispatcherPriority.Background, configuration, load, progressHandler);

            else
            {
                InitializeWork(configuration, load, progressHandler);
            }
        }
        public abstract void Dispose();
    }
}
