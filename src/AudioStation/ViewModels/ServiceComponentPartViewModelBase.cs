using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;

using SimpleWpf.Extensions.Event;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

using static AudioStation.Event.DialogEventHandlers;
using static AudioStation.ViewModels.ServiceComponentDelegates;

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
            protected set { this.RaiseAndSetIfChanged(ref _working, value); RaiseStatusChangedEvent(); }
        }
        public bool Loaded
        {
            get { return _loaded; }
            protected set { this.RaiseAndSetIfChanged(ref _loaded, value); RaiseStatusChangedEvent(); }
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

        /// <summary>
        /// Event that executes when the working status has changed
        /// </summary>
        public event ServiceComponentStatusUpdateHandler StatusChangeEvent;

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

        protected void RaiseStatusChangedEvent()
        {
            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, this.Working, this.Loaded);
        }

        public abstract void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        public abstract void Execute(DialogProgressHandler progressHandler);
        public abstract void Reset(DialogProgressHandler progressHandler);
    }
}
