using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;

using SimpleWpf.Extensions.Event;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;
using SimpleWpf.Utilities;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.ViewModels
{
    /// <summary>
    /// Component base to specify further sub-tasks primarily for the importer
    /// </summary>
    public abstract class ServiceComponentPartViewModelBase : ViewModelBase, IDisposable
    {
        // This will be a read-only part ID to handle back and forth with the
        // owner component(s)
        Guid _id;
        bool _working;
        bool _loaded;
        string _displayName;

        SimpleCommand _executeCommand;
        SimpleCommand _loadCommand;
        SimpleCommand _resetCommand;

        public Guid Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
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
        public SimpleCommand ExecuteCommand
        {
            get { return _executeCommand; }
            set { this.RaiseAndSetIfChanged(ref _executeCommand, value); }
        }
        public SimpleCommand LoadCommand
        {
            get { return _loadCommand; }
            set { this.RaiseAndSetIfChanged(ref _loadCommand, value); }
        }
        public SimpleCommand ResetCommand
        {
            get { return _resetCommand; }
            set { this.RaiseAndSetIfChanged(ref _resetCommand, value); }
        }

        public event SimpleEventHandler<Guid> ExecuteRequestEvent;
        public event SimpleEventHandler<Guid> LoadRequestEvent;
        public event SimpleEventHandler<Guid> ResetRequestEvent;

        public ServiceComponentPartViewModelBase(string displayName)
        {
            this.Id = Guid.NewGuid();
            this.Working = false;
            this.Loaded = false;
            this.DisplayName = displayName;

            this.ExecuteCommand = new SimpleCommand(() =>
            {
                if (this.ExecuteRequestEvent != null)
                    this.ExecuteRequestEvent(this.Id);

            }, CanExecute);
            this.LoadCommand = new SimpleCommand(() =>
            {
                if (this.LoadRequestEvent != null)
                    this.LoadRequestEvent(this.Id);

            }, CanLoad);
            this.ResetCommand = new SimpleCommand(() =>
            {
                if (this.ResetRequestEvent != null)
                    this.ResetRequestEvent(this.Id);

            }, CanReset);
        }

        public abstract bool CanExecute();
        public abstract bool CanLoad();
        public abstract bool CanReset();
        public abstract void Dispose();

        protected abstract void LoadWork(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        protected abstract void ExecuteWork(DialogProgressHandler progressHandler);
        protected abstract void ResetWork(DialogProgressHandler progressHandler);

        /// <summary>
        /// Function to load component view model. This would be called when a a view is loaded; or when needed in the application.
        /// </summary>
        /// <exception cref="Exception">Component must have first been initialized</exception>
        public void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            if (!CanLoad())
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
            if (!CanExecute())
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
            if (!CanReset())
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
