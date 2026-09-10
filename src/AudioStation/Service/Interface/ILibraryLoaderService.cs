using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;
using AudioStation.ViewModels.ComponentViewModels.LoadViewModels;

using SimpleWpf.UI.ViewModel.FileTreeView;

using static AudioStation.Event.DialogEventHandlers;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService : IAudioStationService
    {
        /// <summary>
        /// Adds workflow entity to the database from the view-model
        /// </summary>
        void AddOrUpdateImportWorkflow(LibraryImporterWorkflowViewModel workflow);

        /// <summary>
        /// Gets saved workflow(s) from the database and maps them to the view model namespace
        /// </summary>
        /// <returns></returns>
        IEnumerable<LibraryImporterWorkflowViewModel> GetWorkflows();

        /// <summary>
        /// Initializes Audio Station Library with entities from the database
        /// </summary>
        LibraryViewModel LoadLibrary(DialogProgressHandler progressHandler);

        /// <summary>
        /// Loads a library entry page from the database
        /// </summary>
        PageResult<TrackViewModel> LoadEntryPage(PageRequest<Track, int> request);

        /// <summary>
        /// Initializes the library importer directory to recursion depth 0.
        /// </summary>
        FileTreeViewModel InitializeImporterTree(string directory,
                                                    string searchPattern,
                                                    LibraryImporterConfigurationViewModel importerOptions,
                                                    DialogProgressHandler progressHandler);

        /// <summary>
        /// Loads further directories of the importer tree
        /// </summary>
        void LoadImporterTreeNextDepth(FileTreeViewModel treeRoot,
                                        int currentDepth,
                                        string searchPattern,
                                        DialogProgressHandler progressHandler);
    }
}
