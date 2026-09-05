using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.Core.Utility;
using AudioStation.EventHandler;
using AudioStation.Service.Interface;
using AudioStation.Utility;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryLoaderViewModels.Worker;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.UI.ViewModel.FileTreeView;

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

        public FileTreeViewModel InitializeImporterTree(
                                                string directory,
                                                string searchPattern,
                                                LibraryImporterConfigurationViewModel importerOptions)
        {
            try
            {
                // Load first depth of the tree (TODO: Fix showing only the root, instead of starting with the child nodes)
                return DirectoryTreeLoader.Load(directory, searchPattern, -1, directoryNode =>
                {
                    return new FileTreeViewModel(searchPattern, directoryNode);

                }, (directoryPath, directoryFileCount) =>
                {
                    return new FileTreeNodeViewModel(directory, directoryPath, directoryFileCount);

                }, filePath =>
                {
                    return new FileTreeNodeViewModel(directory, filePath, 0);
                });
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error loading import files:  {0}", LogLevel.Error, ex, ex.Message);
                throw ex;
            }
        }

        public void LoadImporterTreeNextDepth(
                        FileTreeViewModel treeRoot,
                        int currentDepth,
                        string searchPattern)
        {
            try
            {
                DirectoryTreeLoader.LoadToDepth(treeRoot, searchPattern, currentDepth + 1, directoryNode =>
                {
                    return new FileTreeViewModel(searchPattern, directoryNode);

                }, (directoryPath, directoryFileCount) =>
                {
                    return new FileTreeNodeViewModel(treeRoot.NodeValue.FullPath, directoryPath, directoryFileCount);

                }, filePath =>
                {
                    return new FileTreeNodeViewModel(treeRoot.NodeValue.FullPath, filePath, 0);
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
