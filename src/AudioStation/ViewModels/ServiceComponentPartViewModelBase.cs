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
        string _description;

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
            protected set
            {
                if (this.RaiseAndSetIfChanged(ref _working, value))
                    RaiseStatusChangedEvent();
            }
        }
        public bool Loaded
        {
            get { return _loaded; }
            protected set
            {
                if (this.RaiseAndSetIfChanged(ref _loaded, value))
                    RaiseStatusChangedEvent();
            }
        }
        public string DisplayName
        {
            get { return _displayName; }
            protected set { this.RaiseAndSetIfChanged(ref _displayName, value); }
        }
        public string Description
        {
            get { return _description; }
            protected set { this.RaiseAndSetIfChanged(ref _description, value); }
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

        /// <summary>
        /// Event that executes when the work is complete
        /// </summary>
        public event ServiceComponentStatusUpdateHandler WorkCompleteEvent;

        public ServiceComponentPartViewModelBase(string displayName, string description)
        {
            this.Id = Guid.NewGuid();
            this.Working = false;
            this.Loaded = false;
            this.DisplayName = displayName;
            this.Description = description;

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

        protected void RaiseLoadEvent()
        {
            if (this.LoadRequestEvent != null)
                this.LoadRequestEvent(this.Id);
        }
        protected void RaiseExecuteEvent()
        {
            if (this.ExecuteRequestEvent != null)
                this.ExecuteRequestEvent(this.Id);
        }
        protected void RaiseResetEvent()
        {
            if (this.ResetRequestEvent != null)
                this.ResetRequestEvent(this.Id);
        }

        protected void RaiseStatusChangedEvent()
        {
            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, this.Working, this.Loaded);
        }

        protected void RaiseWorkCompleteEvent()
        {
            if (this.WorkCompleteEvent != null)
                this.WorkCompleteEvent(this, this.Working, this.Loaded);
        }

        public abstract void Load(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        public abstract void Execute(DialogProgressHandler progressHandler);
        public abstract void Reset(DialogProgressHandler progressHandler);
    }
}
