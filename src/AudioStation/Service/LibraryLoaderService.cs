using AudioStation.Core.Component.Interface;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Service
{
    [IocExport(typeof(ILibraryLoaderService))]
    public class LibraryLoaderService : ILibraryLoaderService
    {
        private readonly IAudioStationConfigurationManager _audioStationConfigurationManager;

        private readonly LibraryLoaderAcoustIDViewModel _libraryLoaderAcoustIDViewModel;
        private readonly LibraryLoaderFileCheckerViewModel _libraryLoaderFileCheckerViewModel;
        private readonly LibraryLoaderMusicBrainzBasicViewModel _libraryLoaderMusicBrainzBasicViewModel;
        private readonly LibraryLoaderMusicBrainzAlbumArtViewModel _libraryLoaderMusicBrainzAlbumArtViewModel;

        [IocImportingConstructor]
        public LibraryLoaderService(IAudioStationConfigurationManager audioStationConfigurationManager,
                                    LibraryLoaderAcoustIDViewModel libraryLoaderAcoustIDViewModel,
                                    LibraryLoaderFileCheckerViewModel libraryLoaderFileCheckerViewModel,
                                    LibraryLoaderMusicBrainzBasicViewModel libraryLoaderMusicBrainzBasicViewModel,
                                    LibraryLoaderMusicBrainzAlbumArtViewModel libraryLoaderMusicBrainzAlbumArtViewModel)
        {
            _audioStationConfigurationManager = audioStationConfigurationManager;
            _libraryLoaderAcoustIDViewModel = libraryLoaderAcoustIDViewModel;
            _libraryLoaderFileCheckerViewModel = libraryLoaderFileCheckerViewModel;
            _libraryLoaderMusicBrainzBasicViewModel = libraryLoaderMusicBrainzBasicViewModel;
            _libraryLoaderMusicBrainzAlbumArtViewModel = libraryLoaderMusicBrainzAlbumArtViewModel;
        }

        public void Initialize(DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var configuration = _audioStationConfigurationManager.GetConfiguration();

            var taskCount = 4;
            var task = 1;

            // Library Loader: File Checker
            progressHandler(taskCount, task++, 0, "Initializing File Checker...");
            _libraryLoaderFileCheckerViewModel.InitializeWorkItems(configuration, progressHandler);

            // Library Loader: AcoustID
            progressHandler(taskCount, task++, 0, "Initializing AcoustID...");
            _libraryLoaderAcoustIDViewModel.InitializeWorkItems(configuration, progressHandler);

            // Library Loader: Music Brainz (Basic)
            progressHandler(taskCount, task++, 0, "Initializing Music Brainz (Basic)...");
            _libraryLoaderMusicBrainzBasicViewModel.InitializeWorkItems(configuration, progressHandler);

            // Library Loader: Music Brainz (Album Art)
            progressHandler(taskCount, task++, 0, "Initializing Music Brainz (Album Art)...");
            _libraryLoaderMusicBrainzAlbumArtViewModel.InitializeWorkItems(configuration, progressHandler);
        }
    }
}
