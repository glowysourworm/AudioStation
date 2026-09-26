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
        private List<ServiceComponentViewModelBase> _serviceComponents;
        private List<DataComponentViewModelBase> _dataComponents;

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

            _serviceComponents = new List<ServiceComponentViewModelBase>()
            {
                _bandcampViewModel,
                _cdImporterViewModel,
                _libraryImporterViewModel,
                _libraryLoaderViewModel,
                _radioViewModel
            };
            _dataComponents = new List<DataComponentViewModelBase>()
            {
                _libraryManagerViewModel,
                _logViewModel,
                _mainViewModel,
                _nowPlayingViewModel,
                _statusViewModel
            };

            // Configuration Updates
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                if (_audioStationConfigurationViewModel != null &&
                    _dataComponents.Contains(_audioStationConfigurationViewModel))
                    _dataComponents.Remove(_audioStationConfigurationViewModel);

                _audioStationConfigurationViewModel = eventData.ViewModel;
                _configuration = eventData.Configuration;

                if (_audioStationConfigurationViewModel != null)
                    _dataComponents.Add(_audioStationConfigurationViewModel);
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

            foreach (DataComponentViewModelBase component in _dataComponents)
            {
                if (component == _logViewModel ||
                    component == _audioStationConfigurationViewModel)
                    continue;

                progressHandler(taskCount, task++, 0, 0, "Initializing " + component.DisplayName);
                component.Initialize(configuration);
            }

            foreach (ServiceComponentViewModelBase component in _serviceComponents)
            {
                progressHandler(taskCount, task++, 0, 0, "Initializing " + component.DisplayName);
                component.Initialize(configuration, audioStationController, progressHandler);
            }
        }

        public T GetServiceComponent<T>() where T : ServiceComponentViewModelBase
        {
            var type = typeof(T);

            foreach (var component in _serviceComponents)
            {
                if (component.GetType() == type)
                    return (T)component;
            }

            throw new Exception("Component not found, or unhandled:  " + type);
        }
        public T GetDataComponent<T>() where T : DataComponentViewModelBase
        {
            var type = typeof(T);

            foreach (var component in _dataComponents)
            {
                if (component.GetType() == type)
                    return (T)component;
            }

            throw new Exception("Component not found, or unhandled:  " + type);
        }

        public void LoadComponent<T>(bool showProgress) where T : ServiceComponentViewModelBase
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = GetServiceComponent<T>();

            // Dialog (Loading)
            if (showProgress)
            {
                _dialogController.ShowLoading("Loading " + component.DisplayName, progressHandler =>
                {
                    // Load Component
                    component.Load(_configuration, _audioStationController, progressHandler);
                });
            }
            else
            {
                // TODO:
            }
        }
        public void ExecuteComponent<T>(bool showProgress) where T : ServiceComponentViewModelBase
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = GetServiceComponent<T>();

            // Dialog (Loading)
            if (showProgress)
                _dialogController.ShowLoading("Executing " + component.DisplayName, component.Execute);
            else
            {
                // TODO
            }
        }
        public void ResetComponent<T>(bool showProgress) where T : ServiceComponentViewModelBase
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = GetServiceComponent<T>();

            // Dialog (Loading)
            if (showProgress)
                _dialogController.ShowLoading("Executing " + component.DisplayName, component.Reset);
            else
            {
                // TODO
            }
        }
        public Task LoadComponentAsync<T>() where T : ServiceComponentViewModelBase
        {
            return Task.Run(() => LoadComponent<T>(false));
        }
        public Task ExecuteComponentAsync<T>() where T : ServiceComponentViewModelBase
        {
            return Task.Run(() => ExecuteComponent<T>(false));
        }
        public Task ResetComponentAsync<T>() where T : ServiceComponentViewModelBase
        {
            return Task.Run(() => ResetComponent<T>(false));
        }
    }
}
