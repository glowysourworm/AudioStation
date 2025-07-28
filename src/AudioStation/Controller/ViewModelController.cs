using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Bandcamp.Interface;
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

        [IocImportingConstructor]
        public ViewModelController(IConfigurationManager configurationManager,
                                   IModelController modelController,
                                   IDialogController dialogController,
                                   IAudioController audioController,
                                   ITagCacheController tagCacheController,
                                   IViewModelLoader viewModelLoader,
                                   ILibraryImporter libraryImporter,
                                   ILibraryLoaderService libraryLoaderService,
                                   IModelValidationService modelValidationService,
                                   IBandcampClient bandcampClient,
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
                                               audioController, eventAggregator, cdDrive,
                                               configurationManager.GetConfiguration(),
                                               _libraryManagerViewModel, _radioViewModel,
                                               _logViewModel, _libraryLoaderViewModel,
                                               _nowPlayingViewModel, _bandcampViewModel);
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

            progressHandler(10, 0, 0, "Initializing Bandcamp Client...");
            await _bandcampViewModel.Initialize(progressHandler);

            progressHandler(10, 1, 0, "Initializing Now Playing...");
            await _nowPlayingViewModel.Initialize(progressHandler);

            progressHandler(10, 2, 0, "Initializing CD Importer...");
            await _libraryLoaderCDImportViewModel.Initialize(progressHandler);

            progressHandler(10, 3, 0, "Initializing Music Brainz...");
            await _libraryLoaderDownloadMusicBrainzViewModel.Initialize(progressHandler);

            progressHandler(10, 4, 0, "Initializing Radio Importer...");
            await _libraryLoaderImportRadioViewModel.Initialize(progressHandler);

            progressHandler(10, 5, 0, "Initializing Importer...");
            await _libraryLoaderImportViewModel.Initialize(progressHandler);

            progressHandler(10, 6, 0, "Initializing Library Loader...");
            await _libraryLoaderViewModel.Initialize(progressHandler);

            progressHandler(10, 7, 0, "Initializing Logger...");
            await _logViewModel.Initialize(progressHandler);

            progressHandler(10, 8, 0, "Initializing Radio...");
            await _radioViewModel.Initialize(progressHandler);

            // Primary Loading... There would then be navigation to the first "task" for the user:  Configuration Errors, 
            //                    Library Maintenance; or even Now Playing :)
            //

            progressHandler(10, 9, 0, "Initializing Library...");
            await _libraryManagerViewModel.Initialize(progressHandler);

            progressHandler(10, 10, 0, "Initializing User Interface...");
            await _mainViewModel.Initialize(progressHandler);
        }
    }
}
