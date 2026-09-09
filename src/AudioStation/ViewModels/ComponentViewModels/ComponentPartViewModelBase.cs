using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;

using SimpleWpf.UI.ViewModel;
using SimpleWpf.Utilities;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.ViewModels.ComponentViewModels
{
    /// <summary>
    /// Component base to specify further sub-tasks primarily for the importer
    /// </summary>
    public abstract class ComponentPartViewModelBase : ViewModelBase
    {
        bool _working;
        bool _loading;
        bool _initialized;
        string _displayName;

        public bool Working
        {
            get { return _working; }
            set { this.RaiseAndSetIfChanged(ref _working, value); }
        }
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

        public ComponentPartViewModelBase(string displayName)
        {
            this.Working = false;
            this.Initialized = false;
            this.DisplayName = displayName;
        }

        /// <summary>
        /// Function to complete initialization. This will be called on the Dispatcher thread
        /// </summary>
        protected abstract void InitializeImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="viewModelLoader"></param>
        /// <param name="progressHandler"></param>
        protected abstract void LoadImpl(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);

        public void Initialize(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Initialize, DispatcherPriority.Background, configuration, audioStationController, progressHandler);

            else
            {
                this.Loading = true;

                InitializeImpl(configuration, audioStationController, progressHandler);

                // To be used by subclasses
                this.Initialized = true;
                this.Loading = false;
            }
        }

        /// <summary>
        /// Function to load component view model. This would be called when a a view is loaded; or when needed in the application.
        /// </summary>
        /// <exception cref="Exception">Component must have first been initialized</exception>
        public void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            if (!this.Initialized)
                throw new Exception("Must first initialize WorkflowComponentViewModelBase before calling Load");

            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Load, DispatcherPriority.Background, configuration, audioStationController, progressHandler);

            else
            {
                this.Working = true;

                LoadImpl(configuration, audioStationController, progressHandler);

                this.Working = false;
            }
        }
    }
}
