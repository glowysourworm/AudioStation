using AudioStation.Controller.Interface;
using AudioStation.Core;
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
        private LibraryLoaderAcoustIDViewModel _libraryLoaderAcoustIDViewModel;
        private LibraryLoaderFileCheckerViewModel _libraryLoaderFileCheckerViewModel;
        private LibraryLoaderMusicBrainzBasicViewModel _libraryLoaderMusicBrainzBasicViewModel;
        private LibraryLoaderMusicBrainzAlbumArtViewModel _libraryLoaderMusicBrainzAlbumArtViewModel;

        [IocImportingConstructor]
        public LibraryLoaderService()
        {
        }

        public void Initialize(AudioStationConfiguration configuration, IAudioStationViewModelController audioStationViewModelController, DialogEventHandlers.DialogProgressHandler progressHandler)
        {
            var taskCount = 4;
            var task = 1;

            _libraryLoaderAcoustIDViewModel = audioStationViewModelController.GetComponent<LibraryLoaderAcoustIDViewModel>();
            _libraryLoaderFileCheckerViewModel = audioStationViewModelController.GetComponent<LibraryLoaderFileCheckerViewModel>(); ;
            _libraryLoaderMusicBrainzBasicViewModel = audioStationViewModelController.GetComponent<LibraryLoaderMusicBrainzBasicViewModel>(); ;
            _libraryLoaderMusicBrainzAlbumArtViewModel = audioStationViewModelController.GetComponent<LibraryLoaderMusicBrainzAlbumArtViewModel>(); ;

            // Library Loader: File Checker
            progressHandler(taskCount, task++, 0, "Initializing File Checker...");
            _libraryLoaderFileCheckerViewModel.Initialize(configuration, audioStationViewModelController, progressHandler);

            // Library Loader: AcoustID
            progressHandler(taskCount, task++, 0, "Initializing AcoustID...");
            _libraryLoaderAcoustIDViewModel.Initialize(configuration, audioStationViewModelController, progressHandler);

            // Library Loader: Music Brainz (Basic)
            progressHandler(taskCount, task++, 0, "Initializing Music Brainz (Basic)...");
            _libraryLoaderMusicBrainzBasicViewModel.Initialize(configuration, audioStationViewModelController, progressHandler);

            // Library Loader: Music Brainz (Album Art)
            progressHandler(taskCount, task++, 0, "Initializing Music Brainz (Album Art)...");
            _libraryLoaderMusicBrainzAlbumArtViewModel.Initialize(configuration, audioStationViewModelController, progressHandler);
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

        public void LoadImporterTreeNextDepth(
                        LibraryImporterTreeViewModel directory,
                        ILibraryDirectory sourceDirectory,
                        ILibraryDirectory destinationDirectory,
                        string searchPattern,
                        LibraryImporterConfigurationViewModel importerOptions)
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
