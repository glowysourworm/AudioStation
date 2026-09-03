using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Database.AudioStationDatabase.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Event;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels;
using AudioStation.ViewModels.ComponentViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;
using AudioStation.ViewModels.Vendor;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.Controller
{
    [IocExport(typeof(IAudioStationViewModelController))]
    public class AudioStationViewModelController : IAudioStationViewModelController
    {
        private AudioStationConfigurationViewModel _audioStationConfigurationViewModel;
        private readonly BandcampViewModel _bandcampViewModel;
        private readonly CDImporterViewModel _cdImporterViewModel;
        private readonly LibraryImporterViewModel _libraryImporterViewModel;
        private readonly LibraryLoaderAcoustIDViewModel _libraryLoaderAcoustIDViewModel;
        private readonly LibraryLoaderFileCheckerViewModel _libraryLoaderFileCheckerViewModel;
        private readonly LibraryLoaderMusicBrainzBasicViewModel _libraryLoaderMusicBrainzBasicViewModel;
        private readonly LibraryLoaderMusicBrainzAlbumArtViewModel _libraryLoaderMusicBrainzAlbumArtViewModel;
        private readonly LibraryManagerViewModel _libraryManagerViewModel;
        private readonly LogViewModel _logViewModel;
        private readonly MainViewModel _mainViewModel;
        private readonly NowPlayingViewModel _nowPlayingViewModel;
        private readonly RadioViewModel _radioViewModel;
        private readonly StatusViewModel _statusViewModel;


        [IocImportingConstructor]
        public AudioStationViewModelController(IIocEventAggregator eventAggregator,
                                               IAudioStationServiceController audioStationServiceController,
                                               IAudioStationMapper audioStationMapper,
                                               ICDDrive cdDrive,
                                               IDialogController dialogController,
                                               ILibraryLoaderService libraryLoaderService,
                                               ILibraryLoaderWorkerService libraryLoaderWorkerService,
                                               ITagCacheController tagCacheController)
        {
            // This must be initialized by the IAudioStationController
            _audioStationConfigurationViewModel = null;


            var audioStationDbClient = audioStationServiceController.GetComponent<IAudioStationDbClient>();
            var cdImportService = audioStationServiceController.GetComponent<ICDImportService>();
            var bandcampClient = audioStationServiceController.GetComponent<IBandcampClient>();

            _bandcampViewModel = new BandcampViewModel(bandcampClient, eventAggregator);
            _cdImporterViewModel = new CDImporterViewModel(eventAggregator, cdImportService);
            _libraryImporterViewModel = new LibraryImporterViewModel(audioStationMapper, dialogController, eventAggregator, tagCacheController);
            _libraryLoaderAcoustIDViewModel = new LibraryLoaderAcoustIDViewModel(eventAggregator, libraryLoaderWorkerService);
            _libraryLoaderFileCheckerViewModel = new LibraryLoaderFileCheckerViewModel(eventAggregator, libraryLoaderWorkerService, audioStationDbClient);
            _libraryLoaderMusicBrainzBasicViewModel = new LibraryLoaderMusicBrainzBasicViewModel(eventAggregator, libraryLoaderWorkerService, audioStationDbClient);
            _libraryLoaderMusicBrainzAlbumArtViewModel = new LibraryLoaderMusicBrainzAlbumArtViewModel(eventAggregator, libraryLoaderWorkerService, audioStationDbClient);
            _libraryManagerViewModel = new LibraryManagerViewModel(eventAggregator);
            _logViewModel = new LogViewModel(eventAggregator);
            _mainViewModel = new MainViewModel(audioStationServiceController, audioStationMapper, dialogController, eventAggregator, cdDrive);
            _nowPlayingViewModel = new NowPlayingViewModel(eventAggregator);
            _radioViewModel = new RadioViewModel(libraryLoaderWorkerService, dialogController);
            _statusViewModel = new StatusViewModel();

            // Configuration Updates
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(eventData =>
            {
                _audioStationConfigurationViewModel = eventData.ViewModel;
            });
        }

        public T GetComponent<T>() where T : ComponentViewModelBase
        {
            var type = typeof(T);

            // Configuration (lazy loading!)
            if (_audioStationConfigurationViewModel != null && type == _audioStationConfigurationViewModel.GetType())
                return _audioStationConfigurationViewModel as T;

            else if (type == _bandcampViewModel.GetType())
                return _bandcampViewModel as T;

            else if (type == _cdImporterViewModel.GetType())
                return _cdImporterViewModel as T;

            else if (type == _libraryImporterViewModel.GetType())
                return _libraryImporterViewModel as T;

            else if (type == _libraryLoaderAcoustIDViewModel.GetType())
                return _libraryLoaderAcoustIDViewModel as T;

            else if (type == _libraryLoaderFileCheckerViewModel.GetType())
                return _libraryLoaderFileCheckerViewModel as T;

            else if (type == _libraryLoaderMusicBrainzBasicViewModel.GetType())
                return _libraryLoaderMusicBrainzBasicViewModel as T;

            else if (type == _libraryLoaderMusicBrainzAlbumArtViewModel.GetType())
                return _libraryLoaderMusicBrainzAlbumArtViewModel as T;

            else if (type == _libraryManagerViewModel.GetType())
                return _libraryManagerViewModel as T;

            else if (type == _logViewModel.GetType())
                return _logViewModel as T;

            else if (type == _mainViewModel.GetType())
                return _mainViewModel as T;

            else if (type == _nowPlayingViewModel.GetType())
                return _nowPlayingViewModel as T;

            else if (type == _radioViewModel.GetType())
                return _radioViewModel as T;

            else if (type == _statusViewModel.GetType())
                return _statusViewModel as T;

            else
                throw new Exception("View Model not found, or unhandled:  " + type);
        }

        public void Initialize(AudioStationConfiguration configuration, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            _bandcampViewModel.Initialize(configuration, this, progressHandler);
            _cdImporterViewModel.Initialize(configuration, this, progressHandler);
            _libraryImporterViewModel.Initialize(configuration, this, progressHandler);
            _libraryLoaderAcoustIDViewModel.Initialize(configuration, this, progressHandler);
            _libraryLoaderFileCheckerViewModel.Initialize(configuration, this, progressHandler);
            _libraryLoaderMusicBrainzBasicViewModel.Initialize(configuration, this, progressHandler);
            _libraryLoaderMusicBrainzAlbumArtViewModel.Initialize(configuration, this, progressHandler);
            _libraryManagerViewModel.Initialize(configuration, this, progressHandler);
            _logViewModel.Initialize(configuration, this, progressHandler);
            _mainViewModel.Initialize(configuration, this, progressHandler);
            _nowPlayingViewModel.Initialize(configuration, this, progressHandler);
            _radioViewModel.Initialize(configuration, this, progressHandler);
            _statusViewModel.Initialize(configuration, this, progressHandler);
        }
    }
}
