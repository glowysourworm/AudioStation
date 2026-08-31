using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.Utility;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using Microsoft.Extensions.Logging;

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

        public LibraryImporterTreeViewModel InitializeImporterTree(
                    ILibraryDirectory sourceDirectory,
                    ILibraryDirectory destinationDirectory,
                    string searchPattern,
                    LibraryImporterConfigurationViewModel importerOptions)
        {
            try
            {
                // Load first depth of the tree (TODO: Fix showing only the root, instead of starting with the child nodes)
                return DirectoryTreeLoader.Load(sourceDirectory.Directory, searchPattern, 1, directoryNode =>
                {
                    return new LibraryImporterTreeViewModel(directoryNode, searchPattern);

                }, (directoryPath, directoryFileCount) =>
                {
                    return new LibraryImporterDirectoryViewModel(directoryPath, sourceDirectory.Directory, directoryFileCount);

                }, filePath =>
                {
                    return new LibraryImporterFileViewModel(filePath, sourceDirectory, destinationDirectory, importerOptions);
                });
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);

                throw ex;
            }
        }

        public void LoadImporterTreeNextDepth(ref LibraryImporterTreeViewModel directory, ILibraryDirectory sourceDirectory, ILibraryDirectory destinationDirectory, string searchPattern, LibraryImporterConfigurationViewModel importerOptions)
        {
            try
            {
                DirectoryTreeLoader.LoadToDepth(directory, searchPattern, directory.NodeValue.RecursionDepth + 1, directoryNode =>
                {
                    return new LibraryImporterTreeViewModel(directoryNode, searchPattern);

                }, (directoryPath, directoryFileCount) =>
                {
                    return new LibraryImporterDirectoryViewModel(directoryPath, sourceDirectory.Directory, directoryFileCount);

                }, filePath =>
                {
                    return new LibraryImporterFileViewModel(filePath, sourceDirectory, destinationDirectory, importerOptions);
                });
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }
    }
}
