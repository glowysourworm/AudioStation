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
using AudioStation.ViewModels.ComponentViewModels;
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

        // View Models
        private List<ComponentViewModelBase> _components;

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

            // Services
            ILibraryLoaderWorkerService libraryLoaderWorkerService,

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
            _radioViewModel = new RadioViewModel(libraryLoaderWorkerService, dialogController);
            _statusViewModel = new StatusViewModel();

            _components = new List<ComponentViewModelBase>()
            {
                _bandcampViewModel,
                _cdImporterViewModel,
                _libraryImporterViewModel,
                _libraryLoaderViewModel,
                _libraryManagerViewModel,
                _logViewModel,
                _mainViewModel,
                _nowPlayingViewModel,
                _radioViewModel,
                _statusViewModel
            };

            // Configuration Updates
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                if (_audioStationConfigurationViewModel != null &&
                    _components.Contains(_audioStationConfigurationViewModel))
                    _components.Remove(_audioStationConfigurationViewModel);

                _audioStationConfigurationViewModel = eventData.ViewModel;
                _configuration = eventData.Configuration;

                if (_audioStationConfigurationViewModel != null)
                    _components.Add(_audioStationConfigurationViewModel);
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

            var taskCount = _components.Count;
            var task = 1;

            // Log (first)
            progressHandler(taskCount, task++, 0, "Initializing " + _logViewModel.DisplayName);
            _logViewModel.Initialize(configuration, audioStationController, progressHandler);

            // Configuration (may need lazy loading) (currently, there's nothing to do)
            if (_audioStationConfigurationViewModel != null)
            {
                progressHandler(taskCount, task++, 0, "Initializing " + _audioStationConfigurationViewModel.DisplayName);
                _cdImporterViewModel.Initialize(configuration, audioStationController, progressHandler);
            }

            foreach (ComponentViewModelBase component in _components)
            {
                if (component == _logViewModel ||
                    component == _audioStationConfigurationViewModel)
                    continue;

                progressHandler(taskCount, task++, 0, "Initializing " + component.DisplayName);
                component.Initialize(configuration, audioStationController, progressHandler);
            }
        }

        public T GetComponent<T>() where T : ComponentViewModelBase
        {
            var type = typeof(T);

            foreach (var component in _components)
            {
                if (component.GetType() == type)
                    return (T)component;
            }

            throw new Exception("Component not found, or unhandled:  " + type);
        }

        public void LoadComponent<T>() where T : ComponentViewModelBase
        {
            if (_configuration == null)
                throw new Exception("Configuration is not yet loaded. Must load configuration before loading components");

            var component = GetComponent<T>();

            // Dialog (Loading)
            _dialogController.ShowLoading("Loading " + component.DisplayName, progressHandler =>
            {
                // Load Component
                component.Load(_configuration, _audioStationController, progressHandler);
            });
        }

        public Task LoadComponentAsync<T>() where T : ComponentViewModelBase
        {
            return Task.Run(LoadComponent<T>);
        }
    }
}
