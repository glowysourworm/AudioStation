using System.Windows.Threading;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;

using SimpleWpf.UI.ViewModel;
using SimpleWpf.Utilities;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.ViewModels.ComponentViewModels
{
    /// <summary>
    /// View model base for a "primary" view model - which contains major pieces of the
    /// application's data. So, there is a life cycle pattern for handling the data from
    /// a controller. The "Load" data type will be used to send data to the view model.
    /// </summary>
    public abstract class ComponentViewModelBase : ViewModelBase
    {
        bool _loading;
        bool _initialized;
        string _displayName;

        /// <summary>
        /// (TODO: Controller pattern!!!) Component is currently running an operation
        /// </summary>
        public bool Loading
        {
            get { return _loading; }
            set { this.RaiseAndSetIfChanged(ref _loading, value); }
        }
        public bool Initialized
        {
            get { return _initialized; }
            set { this.RaiseAndSetIfChanged(ref _initialized, value); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }

        public ComponentViewModelBase(string displayName)
        {
            this.Loading = false;
            this.Initialized = false;
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Function to complete initialization. This will be called on the Dispatcher thread
        /// </summary>
        protected abstract void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationViewModelController viewModelController, DialogProgressHandler progressHandler);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="viewModelLoader"></param>
        /// <param name="progressHandler"></param>
        protected abstract void LoadImpl(IAudioStationConfiguration configuration, IComponentViewModelLoader viewModelLoader, DialogProgressHandler progressHandler);

        public void Initialize(IAudioStationConfiguration configuration, IAudioStationViewModelController viewModelController, DialogProgressHandler progressHandler)
        {
            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Initialize, DispatcherPriority.Background, configuration, viewModelController, progressHandler);

            else
            {
                this.Loading = true;

                InitializeImpl(configuration, viewModelController, progressHandler);

                // To be used by subclasses
                this.Initialized = true;
                this.Loading = false;
            }
        }

        /// <summary>
        /// Function to load component view model. This would be called when a a view is loaded; or when needed in the application.
        /// </summary>
        /// <exception cref="Exception">Component must have first been initialized</exception>
        public void Load(IAudioStationConfiguration configuration, IComponentViewModelLoader viewModelLoader, DialogProgressHandler progressHandler)
        {
            if (!this.Initialized)
                throw new Exception("Must first initialize ComponentViewModelBase before calling Load");

            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Load, DispatcherPriority.Background, configuration, viewModelLoader, progressHandler);

            else
            {
                this.Loading = true;

                LoadImpl(configuration, viewModelLoader, progressHandler);

                this.Loading = false;
            }
        }
    }
}
