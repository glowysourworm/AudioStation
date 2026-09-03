using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.Interface;
using AudioStation.Event;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioStationController))]
    public class AudioStationController : IAudioStationController
    {
        IIocEventAggregator _eventAggregator;
        IAudioStationMapper _audioStationMapper;
        IAudioStationConfigurationManager _audioStationConfigurationManager;
        IAudioStationServiceController _audioStationServiceController;
        IComponentViewModelLoader _componentViewModelLoader;
        ILibraryLoaderService _libraryLoaderService;

        // Primary Configuration View Model
        AudioStationConfigurationViewModel _audioStationConfigurationViewModel;

        [IocImportingConstructor]
        public AudioStationController(IIocEventAggregator eventAggregator,
                                      IAudioStationMapper audioStationMapper,
                                      IAudioStationConfigurationManager audioStationConfigurationManager,
                                      IAudioStationServiceController audioStationServiceController,
                                      IComponentViewModelLoader componentViewModelLoader,
                                      ILibraryLoaderService libraryLoaderService)
        {
            _audioStationMapper = audioStationMapper;
            _eventAggregator = eventAggregator;
            _audioStationConfigurationManager = audioStationConfigurationManager;
            _audioStationServiceController = audioStationServiceController;
            _componentViewModelLoader = componentViewModelLoader;
            _libraryLoaderService = libraryLoaderService;

            _audioStationConfigurationViewModel = new AudioStationConfigurationViewModel();

            audioStationConfigurationManager.ConfigurationEvent += OnConfigurationEvent;
        }

        public AudioStationConfiguration InitializeConfiguration(string configurationFile, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Configuration
            _audioStationConfigurationManager.Initialize(configurationFile);

            return _audioStationConfigurationManager.GetConfiguration();
        }

        public void Initialize(AudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            // Configuration -> Validate
            //var configuration = _audioStationConfigurationManager.GetConfiguration();
            //var valid = _audioStationConfigurationManager.ValidateConfiguration();

            // Initialize:  Primary Component Initializers -> Primary Components (Initialize)
            //
            _audioStationServiceController.Initialize(configuration, progressHandler);
            _componentViewModelLoader.Initialize(configuration, progressHandler);
            _libraryLoaderService.Initialize(configuration, progressHandler);
        }

        private void OnConfigurationEvent(AudioStationConfiguration configuration, ConfigurationEventType eventType, bool configurationValid)
        {
            // Mapper -> View Model (broadcast to listeners)
            _audioStationMapper.MapOnto(configuration, _audioStationConfigurationViewModel);

            // -> Listeners (AudioStation assembly only) (other listeners will be re-initialized due to new configuration)
            _eventAggregator.GetEvent<ConfigurationEvent>().Publish(new ConfigurationEventData(configuration, _audioStationConfigurationViewModel)
            {
                IsConfigurationValid = configurationValid,
                Type = eventType
            });
        }
    }
}
