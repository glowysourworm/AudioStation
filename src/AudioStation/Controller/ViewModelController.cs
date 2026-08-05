using System.Runtime.CompilerServices;

using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor;
using AudioStation.Core.Component.Vendor.Bandcamp.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Service.Interface;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.LibraryLoaderViewModels.Import;
using AudioStation.ViewModels.Vendor;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Controller
{
    [IocExport(typeof(IViewModelController))]
    public class ViewModelController : IViewModelController
    {
        private readonly IConfigurationManager _configurationManager;

        private readonly MainViewModel _mainViewModel;

        private readonly LibraryManagerViewModel _libraryManagerViewModel;
        private readonly RadioViewModel _radioViewModel;
        private readonly LogViewModel _logViewModel;
        private readonly LibraryLoaderViewModel _libraryLoaderViewModel;
        private readonly LibraryLoaderImportViewModel _libraryLoaderImportViewModel;
        private readonly LibraryLoaderImportRadioViewModel _libraryLoaderImportRadioViewModel;
        private readonly LibraryLoaderDownloadMusicBrainzViewModel _libraryLoaderDownloadMusicBrainzViewModel;
        private readonly LibraryLoaderCDImportViewModel _libraryLoaderCDImportViewModel;
        private readonly NowPlayingViewModel _nowPlayingViewModel;
        private readonly BandcampViewModel _bandcampViewModel;

        // IAudioStationComponent
        private readonly IOutputController _outputController;
        private readonly IAudioController _audioController;
        private readonly IAcoustIDClient _acoustIDClient;
        private readonly IBandcampClient _bandcampClient;
        private readonly IDiscogsClient _discogsClient;
        private readonly IFanartClient _fanartClient;
        private readonly IITunesClient _iTunesClient;
        private readonly ILastFmClient _lastFmClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly ISpotifyClient _spotifyClient;

        [IocImportingConstructor]
        public ViewModelController(IConfigurationManager configurationManager,
                                   IModelController modelController,
                                   IDialogController dialogController,
                                   ITagCacheController tagCacheController,
                                   IViewModelLoader viewModelLoader,
                                   ILibraryImporter libraryImporter,
                                   ILibraryLoaderService libraryLoaderService,
                                   IModelValidationService modelValidationService,

                                   // IAudioStationComponent
                                   IAudioController audioController,
                                   IOutputController outputController,
                                   IAcoustIDClient acoustIDClient,
                                   IBandcampClient bandcampClient,
                                   IDiscogsClient discogsClient,
                                   IFanartClient fanartClient,
                                   IITunesClient itunesClient,
                                   ILastFmClient lastFmClient,
                                   IMusicBrainzClient musicBrainzClient,
                                   ISpotifyClient spotifyClient,

                                   IIocEventAggregator eventAggregator,
                                   ICDImportService importService,
                                   ICDDrive cdDrive)
        {
            _configurationManager = configurationManager;

            _libraryLoaderCDImportViewModel = new LibraryLoaderCDImportViewModel(eventAggregator, importService);

            _libraryLoaderImportViewModel = new LibraryLoaderImportViewModel(configurationManager, 
                                                                             dialogController, 
                                                                             eventAggregator, 
                                                                             libraryImporter,
                                                                             tagCacheController, 
                                                                             viewModelLoader);

            _libraryLoaderImportRadioViewModel = new LibraryLoaderImportRadioViewModel(configurationManager, dialogController);
            _libraryLoaderDownloadMusicBrainzViewModel = new LibraryLoaderDownloadMusicBrainzViewModel(modelController, configurationManager, dialogController);

            _libraryManagerViewModel = new LibraryManagerViewModel(viewModelLoader, eventAggregator);
            _radioViewModel = new RadioViewModel(libraryLoaderService, dialogController);
            _logViewModel = new LogViewModel(eventAggregator);
            _nowPlayingViewModel = new NowPlayingViewModel(eventAggregator);
            _bandcampViewModel = new BandcampViewModel(bandcampClient, eventAggregator);


            _libraryLoaderViewModel = new LibraryLoaderViewModel(configurationManager, eventAggregator,
                                                                 _libraryLoaderCDImportViewModel, _libraryLoaderImportViewModel,
                                                                 _libraryLoaderImportRadioViewModel, _libraryLoaderDownloadMusicBrainzViewModel);

            _mainViewModel = new MainViewModel(configurationManager, dialogController,
                                               eventAggregator, cdDrive, audioController, 
                                               outputController, acoustIDClient, bandcampClient, 
                                               discogsClient, fanartClient, itunesClient, 
                                               lastFmClient, musicBrainzClient, spotifyClient,
                                               configurationManager.GetConfiguration(),
                                               _libraryManagerViewModel, _radioViewModel,
                                               _logViewModel, _libraryLoaderViewModel,
                                               _nowPlayingViewModel, _bandcampViewModel);


            // IAudioStationComponent
            _audioController = audioController;
            _outputController = outputController;
            _acoustIDClient = acoustIDClient;
            _bandcampClient = bandcampClient;
            _discogsClient = discogsClient;
            _fanartClient = fanartClient;
            _iTunesClient = itunesClient;
            _lastFmClient = lastFmClient;
            _musicBrainzClient = musicBrainzClient;
            _spotifyClient = spotifyClient;
        }

        public MainViewModel GetMainViewModel()
        {
            return _mainViewModel;
        }

        public async Task Initialize(DialogProgressHandler progressHandler)
        {
            // Procedure
            // 
            // 1) Load View Models
            //      -> Errors:  Show User / Exit
            //      -> Success: Continue
            //
            // 2) Report between view models
            //

            var taskCount = 19;
            var task = 0;

            progressHandler(taskCount, task++, 0, "Initializing Bandcamp Client...");
            await _bandcampViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Now Playing...");
            await _nowPlayingViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing CD Importer...");
            await _libraryLoaderCDImportViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Music Brainz...");
            await _libraryLoaderDownloadMusicBrainzViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Radio Importer...");
            await _libraryLoaderImportRadioViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Importer...");
            await _libraryLoaderImportViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Library Loader...");
            await _libraryLoaderViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Logger...");
            await _logViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Radio...");
            await _radioViewModel.Initialize(progressHandler);

            // IAudioStationComponent (these display their status on the status bar)
            //
            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _outputController.GetDisplayName()));
            await _outputController.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _audioController.GetDisplayName()));
            await _audioController.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _bandcampClient.GetDisplayName()));
            await _bandcampClient.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _acoustIDClient.GetDisplayName()));
            await _acoustIDClient.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _discogsClient.GetDisplayName()));
            await _discogsClient.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _fanartClient.GetDisplayName()));
            await _fanartClient.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _iTunesClient.GetDisplayName()));
            await _iTunesClient.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _lastFmClient.GetDisplayName()));
            await _lastFmClient.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _musicBrainzClient.GetDisplayName()));
            await _musicBrainzClient.Initialize();

            progressHandler(taskCount, task++, 0, string.Format("Initializing {0}", _spotifyClient.GetDisplayName()));
            await _spotifyClient.Initialize();

            // Primary Loading... There would then be navigation to the first "task" for the user:  Configuration Errors, 
            //                    Library Maintenance; or even Now Playing :)
            //

            progressHandler(taskCount, task++, 0, "Initializing Library...");
            await _libraryManagerViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task, 0, "Initializing User Interface...");
            await _mainViewModel.Initialize(progressHandler);
        }
    }
}
