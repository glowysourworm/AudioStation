using System.Windows.Threading;

using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels;
using AudioStation.ViewModels.DataComponent;
using AudioStation.ViewModels.ServiceComponent;
using AudioStation.ViewModels.Vendor;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioStationComponentController))]
    public class AudioStationComponentController : IAudioStationComponentController
    {
        // Ioc Framework
        private readonly IIocEventAggregator _eventAggregator;

        // Misc Components
        private readonly IAudioStationMapper _audioStationMapper;
        private readonly IDialogController _dialogController;

        // Data Services
        private readonly IAudioStationDbClient _audioStationDbClient;

        // Component View Models
        private AudioStationConfigurationViewModel _audioStationConfigurationViewModel;
        private readonly BandcampViewModel _bandcampViewModel;
        private readonly CDImporterViewModel _cdImporterViewModel;
        private readonly LibraryImporterViewModel _libraryImporterViewModel;
        private readonly LibraryLoaderViewModel _libraryLoaderViewModel;
        private readonly LibraryManagerViewModel _libraryManagerViewModel;
        private readonly LogViewModel _logViewModel;
        private readonly MainViewModel _mainViewModel;
        private readonly NowPlayingViewModel _nowPlayingViewModel;
        private readonly RadioViewModel _radioViewModel;
        private readonly StatusViewModel _statusViewModel;

        // Primary Controller (can't import this one)
        private IAudioStationController _audioStationController;

        // Configuration
        private AudioStationConfiguration? _configuration;

        // View Models (primary services, primary data)
        private Dictionary<Guid, ServiceComponentViewModelBase> _serviceComponents;
        private Dictionary<Guid, DataComponentViewModelBase> _dataComponents;

        [IocImportingConstructor]
        public AudioStationComponentController(

            IIocEventAggregator eventAggregator,
            IDialogController dialogController,
            ITagCache tagCacheController,
            IAudioController audioController,

            // Data Services
            IAudioStationDbClient audioStationDbClient,
            IBandcampClient bandcampClient,
            ICDImportService cdImportService,

            // Core Components
            IAudioStationMapper audioStationMapper,
            ICDDrive cdDrive,
            IAudioConverter audioConverter)
        {
            _eventAggregator = eventAggregator;

            _dialogController = dialogController;
            _audioStationMapper = audioStationMapper;
            _audioStationDbClient = audioStationDbClient;

            // This must be initialized by the IAudioStationController
            _audioStationConfigurationViewModel = null;


            _bandcampViewModel = new BandcampViewModel(bandcampClient, eventAggregator);
            _cdImporterViewModel = new CDImporterViewModel(eventAggregator, cdImportService);
            _libraryImporterViewModel = new LibraryImporterViewModel(audioStationMapper, audioConverter, dialogController, eventAggregator, tagCacheController);
            _libraryLoaderViewModel = new LibraryLoaderViewModel(eventAggregator, audioConverter);
            _libraryManagerViewModel = new LibraryManagerViewModel(eventAggregator);
            _logViewModel = new LogViewModel(eventAggregator);
            _mainViewModel = new MainViewModel(audioController, audioStationMapper, dialogController, eventAggregator, cdDrive, audioConverter);
            _nowPlayingViewModel = new NowPlayingViewModel(eventAggregator);
            _radioViewModel = new RadioViewModel(dialogController);
            _statusViewModel = new StatusViewModel();

            _serviceComponents = new Dictionary<Guid, ServiceComponentViewModelBase>()
            {
                { _bandcampViewModel.Id, _bandcampViewModel },
                { _cdImporterViewModel.Id, _cdImporterViewModel },
                { _libraryImporterViewModel.Id, _libraryImporterViewModel },
                { _libraryLoaderViewModel.Id, _libraryLoaderViewModel },
                { _radioViewModel.Id, _radioViewModel }

            };
            _dataComponents = new Dictionary<Guid, DataComponentViewModelBase>()
            {
                { _libraryManagerViewModel.Id, _libraryManagerViewModel },
                { _logViewModel.Id, _logViewModel },
                { _mainViewModel.Id, _mainViewModel },
                { _nowPlayingViewModel.Id, _nowPlayingViewModel },
                { _statusViewModel.Id, _statusViewModel }
            };

            // Configuration Updates
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                if (_audioStationConfigurationViewModel != null &&
                    _dataComponents.ContainsKey(eventData.ViewModel.Id))
                    _dataComponents.Remove(eventData.ViewModel.Id);

                _audioStationConfigurationViewModel = eventData.ViewModel;
                _configuration = eventData.Configuration;

                if (_audioStationConfigurationViewModel != null)
                    _dataComponents.Add(_audioStationConfigurationViewModel.Id, _audioStationConfigurationViewModel);
            });

            eventAggregator.GetEvent<ServiceComponentRequestEvent>().Subscribe(eventData =>
            {
                switch (eventData.Type)
                {
                    case ServiceComponentRequestType.Execute:
                        ExecuteComponent(eventData.ComponentId, eventData.ShowProgress, eventData.ComponentPartId);
                        break;
                    case ServiceComponentRequestType.Load:
                        LoadComponent(eventData.ComponentId, eventData.ShowProgress, eventData.ComponentPartId);
                        break;
                    case ServiceComponentRequestType.Reset:
                        ResetComponent(eventData.ComponentId, eventData.ShowProgress, eventData.ComponentPartId);
                        break;
                    default:
                        throw new Exception("Unhandled service component request type");
                }
            });
        }

        public void Initialize(AudioStationConfiguration configuration, IAudioStationController audioStationController, DialogProgressHandler progressHandler)
        {
            // Primary Controller! (can't import)
            _audioStationController = audioStationController;

            // Store Configuration
            _configuration = configuration;

            // Procedure:  The only consideration here is ordering the view models:  Log,
            //             Configuration, then the rest of the initializers.
            //

            var taskCount = _serviceComponents.Count + _dataComponents.Count;
            var task = 1;

            // Log (first)
            progressHandler(taskCount, task++, 0, 0, "Initializing " + _logViewModel.DisplayName);
            _logViewModel.Initialize(configuration);

            // Configuration (may need lazy loading) (currently, there's nothing to do)
            if (_audioStationConfigurationViewModel != null)
            {
                progressHandler(taskCount, task++, 0, 0, "Initializing " + _audioStationConfigurationViewModel.DisplayName);
                _audioStationConfigurationViewModel.Initialize(configuration);
            }

            foreach (DataComponentViewModelBase component in _dataComponents.Values)
            {
                if (component == _logViewModel ||
                    component == _audioStationConfigurationViewModel)
                    continue;

                progressHandler(taskCount, task++, 0, 0, "Initializing " + component.DisplayName);
                component.Initialize(configuration);
            }

            foreach (ServiceComponentViewModelBase component in _serviceComponents.Values)
            {
                progressHandler(taskCount, task++, 0, 0, "Initializing " + component.DisplayName);
                component.Initialize(configuration, audioStationController, progressHandler);
            }
        }

        public T GetServiceComponent<T>() where T : ServiceComponentViewModelBase
        {
            var type = typeof(T);

            foreach (var component in _serviceComponents.Values)
            {
                if (component.GetType() == type)
                    return (T)component;
            }

            throw new Exception("Component not found, or unhandled:  " + type);
        }
        public T GetDataComponent<T>() where T : DataComponentViewModelBase
        {
            var type = typeof(T);

            foreach (var component in _dataComponents.Values)
            {
                if (component.GetType() == type)
                    return (T)component;
            }

            throw new Exception("Component not found, or unhandled:  " + type);
        }

        private void LoadComponent(Guid componentId, bool showProgress, Guid? componentPartId = null)
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = _serviceComponents[componentId];

            // Dialog (Loading)
            if (showProgress)
            {
                _dialogController.ShowLoading("Loading " + component.DisplayName, progressHandler =>
                {
                    // Load Component
                    component.Load(componentPartId, _configuration, _audioStationController, progressHandler);
                });
            }
            else
            {
                component.Load(componentPartId, _configuration, _audioStationController, (x, y, z, w, a) => { });
            }
        }
        private void ExecuteComponent(Guid componentId, bool showProgress, Guid? componentPartId = null)
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = _serviceComponents[componentId];

            // Dialog (Loading)
            if (showProgress)
            {
                _dialogController.ShowLoading("Executing " + component.DisplayName, progressHandler =>
                {
                    component.Execute(componentPartId, progressHandler);
                });
            }
            else
            {
                component.Execute(componentPartId, (x, y, z, w, a) => { });
            }
        }
        private void ResetComponent(Guid componentId, bool showProgress, Guid? componentPartId = null)
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = _serviceComponents[componentId];

            // Dialog (Loading)
            if (showProgress)
            {
                _dialogController.ShowLoading("Resetting " + component.DisplayName, progressHandler =>
                {
                    component.Reset(componentPartId, progressHandler);
                });
            }
            else
            {
                component.Reset(componentPartId, (x, y, z, w, a) => { });
            }
        }
        public void LoadComponent<T>(bool showProgress, Guid? componentPartId = null) where T : ServiceComponentViewModelBase
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = GetServiceComponent<T>();

            LoadComponent(component.Id, showProgress, componentPartId);
        }
        public void ExecuteComponent<T>(bool showProgress, Guid? componentPartId = null) where T : ServiceComponentViewModelBase
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = GetServiceComponent<T>();

            ExecuteComponent(component.Id, showProgress, componentPartId);
        }
        public void ResetComponent<T>(bool showProgress, Guid? componentPartId = null) where T : ServiceComponentViewModelBase
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = GetServiceComponent<T>();

            ResetComponent(component.Id, showProgress, componentPartId);
        }
        public Task LoadComponentAsync<T>(bool showProgress, Guid? componentPartId = null) where T : ServiceComponentViewModelBase
        {
            return Task.Run(() =>
            {
                if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                    BasicHelpers.InvokeDispatcher(LoadComponent<T>, DispatcherPriority.Background, showProgress, componentPartId);
                else
                    LoadComponent<T>(showProgress, componentPartId);
            });
        }
        public Task ExecuteComponentAsync<T>(bool showProgress, Guid? componentPartId = null) where T : ServiceComponentViewModelBase
        {
            return Task.Run(() =>
            {
                if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                    BasicHelpers.InvokeDispatcher(ExecuteComponent<T>, DispatcherPriority.Background, showProgress, componentPartId);
                else
                    ExecuteComponent<T>(showProgress, componentPartId);
            });
        }
        public Task ResetComponentAsync<T>(bool showProgress, Guid? componentPartId = null) where T : ServiceComponentViewModelBase
        {
            return Task.Run(() =>
            {
                if (BasicHelpers.IsDispatcher() == ApplicationIsDispatcherResult.False)
                    BasicHelpers.InvokeDispatcher(ResetComponent<T>, DispatcherPriority.Background, showProgress, componentPartId);
                else
                    ResetComponent<T>(showProgress, componentPartId);
            });
        }
    }
}
