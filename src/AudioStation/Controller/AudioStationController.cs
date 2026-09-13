using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.Interface;
using AudioStation.Event;
using AudioStation.Service.Interface;
using AudioStation.ViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioStationController))]
    public class AudioStationController : IAudioStationController
    {
        private readonly IDialogController _dialogController;
        private readonly IIocEventAggregator _eventAggregator;
        private readonly IAudioStationMapper _audioStationMapper;
        private readonly IAudioStationConfigurationController _audioStationConfigurationManager;
        private readonly IAudioStationServiceController _audioStationServiceController;
        private readonly IAudioStationComponentController _audioStationComponentController;
        private readonly ILibraryLoaderService _libraryLoaderService;

        // Primary Configuration View Model
        AudioStationConfigurationViewModel _audioStationConfigurationViewModel;

        #region (public) IAudioStationController
        public IAudioStationConfigurationController ConfigurationController { get { return _audioStationConfigurationManager; } }
        public IAudioStationServiceController ServiceController { get { return _audioStationServiceController; } }
        public IAudioStationComponentController ComponentController { get { return _audioStationComponentController; } }
        public ILibraryLoaderService LibraryLoaderService { get { return _libraryLoaderService; } }
        public IDialogController DialogController { get { return _dialogController; } }
        public IIocEventAggregator EventAggregator { get { return _eventAggregator; } }
        #endregion

        [IocImportingConstructor]
        public AudioStationController(IIocEventAggregator eventAggregator,
                                      IDialogController dialogController,
                                      IAudioStationMapper audioStationMapper,
                                      IAudioStationConfigurationController audioStationConfigurationManager,
                                      IAudioStationServiceController audioStationServiceController,
                                      IAudioStationComponentController componentViewModelLoader,
                                      ILibraryLoaderService libraryLoaderService)
        {
            _audioStationMapper = audioStationMapper;
            _dialogController = dialogController;
            _eventAggregator = eventAggregator;
            _audioStationConfigurationManager = audioStationConfigurationManager;
            _audioStationServiceController = audioStationServiceController;
            _audioStationComponentController = componentViewModelLoader;
            _libraryLoaderService = libraryLoaderService;

            _audioStationConfigurationViewModel = new AudioStationConfigurationViewModel();

            // Configuration (source)
            audioStationConfigurationManager.ConfigurationEvent += OnConfigurationEvent;

            // Configuration (target)
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                switch (eventData.Type)
                {
                    // Configuration (source) 
                    case ConfigurationEventType.Opened:
                    case ConfigurationEventType.Modified:
                    case ConfigurationEventType.Saved:
                        break;

                    // Configuration (target) (these had different meanings.. I guess it really doesn't matter)
                    case ConfigurationEventType.ModifyRequest:
                    case ConfigurationEventType.SaveRequest:

                        // Mapper -> View Model (broadcast to listeners)
                        var configuration = audioStationConfigurationManager.GetConfiguration();

                        // -> AudioStationConfiguration (source)
                        _audioStationMapper.MapOnto(_audioStationConfigurationViewModel, configuration);

                        // Save -> Broadcast
                        audioStationConfigurationManager.SaveConfiguration();

                        break;
                    default:
                        throw new Exception("Unhandled configuration event type");
                }
            });
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
            _audioStationServiceController.Initialize(configuration, this, progressHandler);
            _audioStationComponentController.Initialize(configuration, this, progressHandler);
        }

        private void OnConfigurationEvent(AudioStationConfiguration configuration, ConfigurationEventType eventType, bool configurationValid)
        {
            // Mapper -> View Model (broadcast to listeners)
            _audioStationMapper.MapOnto(configuration, _audioStationConfigurationViewModel);

            // -> Listeners (AudioStation assembly only) (other listeners will be re-initialized due to new configuration)
            _eventAggregator.GetEvent<ConfigurationEvent>().Publish(new ConfigurationEventData()
            {
                Configuration = configuration,
                ViewModel = _audioStationConfigurationViewModel,
                IsConfigurationValid = configurationValid,
                Type = eventType
            });
        }
    }
}
