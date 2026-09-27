using AudioStation.Controller.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Event;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.UI.Command;
using SimpleWpf.UI.ViewModel;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.ViewModels
{
    /// <summary>
    /// View model base for a "primary" view model - which contains major pieces of the
    /// application's data. So, there is a life cycle pattern for handling the data from
    /// a controller. The "Load" data type will be used to send data to the view model.
    /// </summary>
    public abstract class ServiceComponentViewModelBase : ViewModelBase, IDisposable
    {
        private IIocEventAggregator _eventAggregator;

        Guid _id;
        bool _loading;
        bool _loaded;
        bool _initialized;
        string _displayName;

        SimpleCommand _executeCommand;
        SimpleCommand _loadCommand;
        SimpleCommand _resetCommand;

        KeyedObservableCollection<Guid, ServiceComponentPartViewModelBase> _componentParts;

        public Guid Id
        {
            get { return _id; }
            private set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public bool Loading
        {
            get { return _loading; }
            protected set { this.RaiseAndSetIfChanged(ref _loading, value); OnStatusChanged(); }
        }
        public bool Loaded
        {
            get { return _loaded; }
            protected set { this.RaiseAndSetIfChanged(ref _loaded, value); OnStatusChanged(); }
        }
        public bool Initialized
        {
            get { return _initialized; }
            protected set { this.RaiseAndSetIfChanged(ref _initialized, value); }
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

        public IReadOnlyCollection<ServiceComponentPartViewModelBase> ComponentParts
        {
            get { return _componentParts; }
        }

        public ServiceComponentViewModelBase(string displayName)
        {
            _eventAggregator = IocContainer.Get<IIocEventAggregator>();

            this.Id = Guid.NewGuid();
            this.Loading = false;
            this.Initialized = false;
            this.DisplayName = displayName;

            _componentParts = new KeyedObservableCollection<Guid, ServiceComponentPartViewModelBase>();

            // Execute Command (component level)
            //
            this.ExecuteCommand = new SimpleCommand(() =>
            {
                _eventAggregator.GetEvent<ServiceComponentRequestEvent>().Publish(new ServiceComponentRequestData()
                {
                    ComponentId = this.Id,
                    ComponentPartId = null,
                    Type = ServiceComponentRequestType.Execute
                });

            }, CanExecute);

            // Load Command (component level)
            //
            this.LoadCommand = new SimpleCommand(() =>
            {
                _eventAggregator.GetEvent<ServiceComponentRequestEvent>().Publish(new ServiceComponentRequestData()
                {
                    ComponentId = this.Id,
                    ComponentPartId = null,
                    Type = ServiceComponentRequestType.Load
                });

            }, CanLoad);

            // Reset Command (component level)
            //
            this.ResetCommand = new SimpleCommand(() =>
            {
                _eventAggregator.GetEvent<ServiceComponentRequestEvent>().Publish(new ServiceComponentRequestData()
                {
                    ComponentId = this.Id,
                    ComponentPartId = null,
                    Type = ServiceComponentRequestType.Reset
                });

            }, CanReset);
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            // -> Update Command Bindings
            if (this.ExecuteCommand != null)
                this.ExecuteCommand.RaiseCanExecuteChanged();

            if (this.LoadCommand != null)
                this.LoadCommand.RaiseCanExecuteChanged();

            if (this.ResetCommand != null)
                this.ResetCommand.RaiseCanExecuteChanged();
        }

        public abstract bool CanExecute();
        public abstract bool CanReset();
        public abstract bool CanLoad();

        protected void AddComponentPart(ServiceComponentPartViewModelBase part)
        {
            part.LoadRequestEvent += Part_LoadRequestEvent;
            part.ExecuteRequestEvent += Part_ExecuteRequestEvent;
            part.ResetRequestEvent += Part_ResetRequestEvent;
            part.StatusChangeEvent += Part_StatusChangeEvent;

            _componentParts.Add(part.Id, part);
        }

        protected void Part_StatusChangeEvent(ServiceComponentPartViewModelBase sender, bool working, bool loaded)
        {
            this.Loading = _componentParts.Any(x => x.Working);
            this.Loaded = _componentParts.Any(x => x.Loaded);
        }

        protected virtual void OnStatusChanged()
        {
            // Hook for inherited class
        }

        private void Part_ResetRequestEvent(Guid partId)
        {
            _eventAggregator.GetEvent<ServiceComponentRequestEvent>().Publish(new ServiceComponentRequestData()
            {
                ComponentId = this.Id,
                ComponentPartId = null,
                Type = ServiceComponentRequestType.Reset
            });
        }

        private void Part_ExecuteRequestEvent(Guid partId)
        {
            _eventAggregator.GetEvent<ServiceComponentRequestEvent>().Publish(new ServiceComponentRequestData()
            {
                ComponentId = this.Id,
                ComponentPartId = null,
                Type = ServiceComponentRequestType.Execute
            });
        }

        private void Part_LoadRequestEvent(Guid partId)
        {
            _eventAggregator.GetEvent<ServiceComponentRequestEvent>().Publish(new ServiceComponentRequestData()
            {
                ComponentId = this.Id,
                ComponentPartId = null,
                Type = ServiceComponentRequestType.Load
            });
        }

        public abstract void Initialize(IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        public abstract void Load(Guid? componentPartId, IAudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler);
        public abstract void Execute(Guid? componentPartId, DialogProgressHandler progressHandler);
        public abstract void Reset(Guid? componentPartId, DialogProgressHandler progressHandler);

        public virtual void Dispose()
        {
            foreach (ServiceComponentPartViewModelBase part in _componentParts)
            {
                part.Dispose();
            }

            // We will want to detach some memory after large pieces of work { import, library loader, ... }
            _componentParts.Clear();
        }
    }
}
