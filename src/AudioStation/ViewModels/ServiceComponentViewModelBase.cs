using System.Collections.ObjectModel;
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
    /// View model base for a "primary" view model - which contains major pieces of the
    /// application's data. So, there is a life cycle pattern for handling the data from
    /// a controller. The "Load" data type will be used to send data to the view model.
    /// </summary>
    public abstract class ServiceComponentViewModelBase : ViewModelBase, IDisposable
    {
        Guid _id;
        bool _loading;
        bool _loaded;
        bool _initialized;
        string _displayName;

        SimpleCommand _executeCommand;
        SimpleCommand _loadCommand;
        SimpleCommand _resetCommand;

        ObservableCollection<ServiceComponentPartViewModelBase> _componentParts;

        public event SimpleEventHandler<ServiceComponentId> ExecuteRequestEvent;
        public event SimpleEventHandler<ServiceComponentId> LoadRequestEvent;
        public event SimpleEventHandler<ServiceComponentId> ResetRequestEvent;

        public Guid Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
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
            this.Loading = false;
            this.Initialized = false;
            this.DisplayName = displayName;

            _componentParts = new ObservableCollection<ServiceComponentPartViewModelBase>();

            // Execute Command (component level)
            //
            this.ExecuteCommand = new SimpleCommand(() =>
            {
                if (this.ExecuteRequestEvent != null)
                {
                    this.ExecuteRequestEvent(new ServiceComponentId()
                    {
                        ComponentId = this.Id
                    });
                }

            }, CanExecute);

            // Load Command (component level)
            //
            this.LoadCommand = new SimpleCommand(() =>
            {
                if (this.LoadRequestEvent != null)
                {
                    this.LoadRequestEvent(new ServiceComponentId()
                    {
                        ComponentId = this.Id
                    });
                }

            }, CanLoad);

            // Reset Command (component level)
            //
            this.ResetCommand = new SimpleCommand(() =>
            {
                if (this.ResetRequestEvent != null)
                {
                    this.ResetRequestEvent(new ServiceComponentId()
                    {
                        ComponentId = this.Id
                    });
                }

            }, CanReset);
        }

        protected override void OnPropertyChanged(string name)
        {
            base.OnPropertyChanged(name);

            // -> Update Command Bindings
            if (this.ExecuteCommand != null)
                this.ExecuteCommand.RaiseCanExecuteChanged();
        }

        public abstract bool CanExecute();
        public abstract bool CanReset();
        public abstract bool CanLoad();

        protected void AddComponentPart(ServiceComponentPartViewModelBase part)
        {
            part.LoadRequestEvent += Part_LoadRequestEvent;
            part.ExecuteRequestEvent += Part_ExecuteRequestEvent;
            part.ResetRequestEvent += Part_ResetRequestEvent;

            _componentParts.Add(part);
        }

        private void Part_ResetRequestEvent(Guid partId)
        {
            if (this.ResetRequestEvent != null)
                this.ResetRequestEvent(new ServiceComponentId()
                {
                    ComponentId = this.Id,
                    ComponentPartId = partId
                });
        }

        private void Part_ExecuteRequestEvent(Guid partId)
        {
            if (this.ExecuteRequestEvent != null)
                this.ExecuteRequestEvent(new ServiceComponentId()
                {
                    ComponentId = this.Id,
                    ComponentPartId = partId
                });
        }

        private void Part_LoadRequestEvent(Guid partId)
        {
            if (this.LoadRequestEvent != null)
                this.LoadRequestEvent(new ServiceComponentId()
                {
                    ComponentId = this.Id,
                    ComponentPartId = partId
                });
        }

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

        public virtual void Dispose()
        {
            foreach (var part in _componentParts)
            {
                part.Dispose();
            }

            // We will want to detach some memory after large pieces of work { import, library loader, ... }
            _componentParts.Clear();
        }
    }
}
