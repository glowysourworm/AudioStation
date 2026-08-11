using AudioStation.Component.Interface;
using AudioStation.Controller.Interface;
using AudioStation.Core.Component.CDPlayer.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Service.Interface;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryImporterViewModels.Import;
using AudioStation.ViewModels.LibraryLoaderViewModels;
using AudioStation.ViewModels.Vendor;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Controller
{
    [IocExport(typeof(IViewModelController))]
    public class ViewModelController : IViewModelController
    {
        private readonly MainViewModel _mainViewModel;

        private readonly LibraryManagerViewModel _libraryManagerViewModel;
        private readonly StatusViewModel _statusViewModel;
        private readonly RadioViewModel _radioViewModel;
        private readonly LogViewModel _logViewModel;
        private readonly LibraryLoaderViewModel _libraryLoaderViewModel;
        private readonly LibraryImporterViewModel _libraryImporterViewModel;
        private readonly LibraryImporterRadioViewModel _libraryLoaderImportRadioViewModel;
        private readonly LibraryLoaderDownloadMusicBrainzViewModel _libraryLoaderDownloadMusicBrainzViewModel;
        private readonly LibraryLoaderCDImportViewModel _libraryLoaderCDImportViewModel;
        private readonly NowPlayingViewModel _nowPlayingViewModel;
        private readonly BandcampViewModel _bandcampViewModel;

        [IocImportingConstructor]
        public ViewModelController(IConfigurationManager configurationManager,
                                   IAudioStationComponentController audioStationComponentController,
                                   IModelController modelController,
                                   IDialogController dialogController,
                                   ITagCacheController tagCacheController,
                                   IViewModelLoader viewModelLoader,
                                   ILibraryImporter libraryImporter,
                                   ILibraryLoaderService libraryLoaderService,
                                   IModelValidationService modelValidationService,
                                   IIocEventAggregator eventAggregator,
                                   ICDImportService importService,
                                   ICDDrive cdDrive)
        {
            _libraryLoaderCDImportViewModel = new LibraryLoaderCDImportViewModel(eventAggregator, importService);

            _libraryImporterViewModel = new LibraryImporterViewModel(configurationManager,
                                                                        dialogController,
                                                                        eventAggregator,
                                                                        libraryImporter,
                                                                        tagCacheController,
                                                                        viewModelLoader);

            _libraryLoaderImportRadioViewModel = new LibraryImporterRadioViewModel(configurationManager, dialogController);
            _libraryLoaderDownloadMusicBrainzViewModel = new LibraryLoaderDownloadMusicBrainzViewModel(modelController, configurationManager, dialogController);

            _libraryManagerViewModel = new LibraryManagerViewModel(viewModelLoader, eventAggregator);
            _statusViewModel = new StatusViewModel();
            _radioViewModel = new RadioViewModel(libraryLoaderService, dialogController);
            _logViewModel = new LogViewModel(eventAggregator);
            _nowPlayingViewModel = new NowPlayingViewModel(eventAggregator);
            _bandcampViewModel = new BandcampViewModel(audioStationComponentController.GetComponent<IBandcampClient>(), eventAggregator);


            _libraryLoaderViewModel = new LibraryLoaderViewModel(configurationManager, eventAggregator,
                                                                 _libraryLoaderCDImportViewModel,
                                                                 _libraryLoaderImportRadioViewModel, _libraryLoaderDownloadMusicBrainzViewModel);

            _mainViewModel = new MainViewModel(configurationManager, audioStationComponentController, dialogController,
                                               eventAggregator, cdDrive,
                                               configurationManager.GetConfiguration(),
                                               _libraryManagerViewModel, _statusViewModel, _radioViewModel,
                                               _logViewModel, _libraryLoaderViewModel, _libraryImporterViewModel,
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

            var taskCount = 10;
            var task = 0;

            progressHandler(taskCount, task++, 0, "Initializing Status Component...");
            await _bandcampViewModel.Initialize(progressHandler);

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
            await _libraryImporterViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Library Loader...");
            await _libraryLoaderViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Logger...");
            await _logViewModel.Initialize(progressHandler);

            progressHandler(taskCount, task++, 0, "Initializing Radio...");
            await _radioViewModel.Initialize(progressHandler);

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
