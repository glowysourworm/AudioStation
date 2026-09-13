using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;

using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;
using SimpleWpf.Utilities;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.ViewModels.ComponentViewModels
{
    /// <summary>
    /// View model base for a "primary" view model - which contains major pieces of the
    /// application's data. So, there is a life cycle pattern for handling the data from
    /// a controller. The "Load" data type will be used to send data to the view model.
    /// </summary>
    public abstract class ComponentViewModelBase : ViewModelBase
    {
        private IDialogController _dialogController;

        bool _loading;
        bool _loaded;
        bool _initialized;
        string _displayName;

        SimpleCommand _executeCommand;

        /// <summary>
        /// (TODO: Controller pattern!!!) Component is currently running an operation
        /// </summary>
        public bool Loading
        {
            get { return _loading; }
            private set { this.RaiseAndSetIfChanged(ref _loading, value); }
        }
        public bool Loaded
        {
            get { return _loaded; }
            private set { this.RaiseAndSetIfChanged(ref _loaded, value); }
        }
        public bool Initialized
        {
            get { return _initialized; }
            private set { this.RaiseAndSetIfChanged(ref _initialized, value); }
        }
        public string DisplayName
        {
            get { return _displayName; }
            private set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }

        public SimpleCommand ExecuteCommand
        {
            get { return _executeCommand; }
            set { this.RaiseAndSetIfChanged(ref _executeCommand, value); }
        }

        public ComponentViewModelBase(string displayName)
        {
            this.Loading = false;
            this.Initialized = false;
            this.DisplayName = displayName;

            this.ExecuteCommand = new SimpleCommand(() =>
            {
                _dialogController.ShowLoading(this.DisplayName + " Loading...", progressHandler =>
                {
                    Execute(progressHandler);
                });

            }, () => CanExecute() && this.Initialized);
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            // -> Update Command Bindings
            if (this.ExecuteCommand != null)
                this.ExecuteCommand.RaiseCanExecuteChanged();
        }

        public abstract bool CanExecute();

        protected abstract void InitializeWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        protected abstract void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        protected abstract void ExecuteWork(DialogProgressHandler progressHandler);
        protected abstract void ResetWork(DialogProgressHandler progressHandler);

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
                _dialogController = audioStationController.DialogController;

                this.Loading = true;

                InitializeWork(configuration, audioStationController, progressHandler);

                // To be used by subclasses
                this.Initialized = true;
                this.Loading = false;
            }
        }
        public void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            if (!this.Initialized)
                throw new Exception("Must first initialize ComponentViewModelBase before calling Load");

            if (this.Loaded)
                throw new Exception("ComponentViewModelBase is already loaded");

            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Load, DispatcherPriority.Background, configuration, audioStationController, progressHandler);

            else
            {
                this.Loading = true;

                LoadWork(configuration, audioStationController, progressHandler);

                this.Loading = false;
                this.Loaded = true;
            }
        }
        public void Execute(DialogProgressHandler progressHandler)
        {
            if (!this.Initialized)
                throw new Exception("Must first initialize ComponentViewModelBase before calling Execute");

            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Execute, DispatcherPriority.Background, progressHandler);

            else
            {
                this.Loading = true;

                ExecuteWork(progressHandler);

                this.Loading = false;
            }
        }
        public void Reset(DialogProgressHandler progressHandler)
        {
            if (!this.Initialized)
                throw new Exception("Must first initialize ComponentViewModelBase before calling Execute");

            // Synchronous Invoke:  This should be used where there is no (async / await). Also, it is needed for completing the work during
            //                      the application's initialization waiter. So, there is already a waiter for this load; but the work must
            //                      be completed on the main thread because of view model binding.
            //
            if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                BasicHelpers.InvokeDispatcher(Reset, DispatcherPriority.Background, progressHandler);

            else
            {
                this.Loading = true;

                ResetWork(progressHandler);

                this.Loading = false;
                this.Loaded = false;
            }
        }
    }
}
