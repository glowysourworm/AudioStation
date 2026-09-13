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
        bool _loaded;
        string _displayName;

        public bool Working
        {
            get { return _working; }
            protected set { this.RaiseAndSetIfChanged(ref _working, value); }
        }
        public bool Loaded
        {
            get { return _loaded; }
            protected set { this.RaiseAndSetIfChanged(ref _loaded, value); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            protected set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }

        public ComponentPartViewModelBase(string displayName)
        {
            this.Working = false;
            this.Loaded = false;
            this.DisplayName = displayName;
        }

        public abstract bool CanExecute();

        protected abstract void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        protected abstract void ExecuteWork(DialogProgressHandler progressHandler);
        protected abstract void ResetWork(DialogProgressHandler progressHandler);

        /// <summary>
        /// Function to load component view model. This would be called when a a view is loaded; or when needed in the application.
        /// </summary>
        /// <exception cref="Exception">Component must have first been initialized</exception>
        public void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            if (this.Loaded)
                throw new Exception("Component already loaded. Must call Reset(..) before reloading the component");

            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Load, DispatcherPriority.Background, configuration, audioStationController, progressHandler);

            else
            {
                this.Working = true;

                LoadWork(configuration, audioStationController, progressHandler);

                this.Working = false;
                this.Loaded = true;
            }
        }

        public void Execute(DialogProgressHandler progressHandler)
        {
            if (!this.Loaded)
                throw new Exception("Component not yet loaded. Must first load the component part before calling Execute()");

            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Execute, DispatcherPriority.Background, progressHandler);

            else
            {
                this.Working = true;

                ExecuteWork(progressHandler);

                this.Working = false;
                this.Loaded = false;
            }
        }

        public void Reset(DialogProgressHandler progressHandler)
        {
            if (!this.Loaded)
                throw new Exception("Component not yet loaded. Must first load the component part before calling Reset()");

            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Reset, DispatcherPriority.Background, progressHandler);

            else
            {
                this.Working = true;

                ResetWork(progressHandler);

                this.Working = false;
                this.Loaded = false;
            }
        }
    }
}
