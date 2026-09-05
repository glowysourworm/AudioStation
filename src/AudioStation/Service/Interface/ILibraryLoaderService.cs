using AudioStation.Controller.Interface;
using AudioStation.Core;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;

using SimpleWpf.UI.ViewModel.FileTreeView;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService
    {
        /// <summary>
        /// Initialization of the view model - this should be run during startup
        /// </summary>
        void Initialize(AudioStationConfiguration configuration, IAudioStationViewModelController audioStationViewModelController, DialogProgressHandler progressHandler);

        /// <summary>
        /// Initializes the library importer directory to recursion depth 0.
        /// </summary>
        public FileTreeViewModel InitializeImporterTree(string directory,
                                                        string searchPattern,
                                                        LibraryImporterConfigurationViewModel importerOptions,
                                                        DialogProgressHandler progressHandler);

        /// <summary>
        /// Loads further directories of the importer tree
        /// </summary>
        public void LoadImporterTreeNextDepth(FileTreeViewModel treeRoot,
                                              int currentDepth,
                                              string searchPattern,
                                              DialogProgressHandler progressHandler);
    }
}
