using AudioStation.Core.Model.Interface;
using AudioStation.ViewModels.ComponentViewModels.LibraryImporterViewModels.Import;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Service.Interface
{
    public interface ILibraryLoaderService
    {
        /// <summary>
        /// Initialization of the view model - this should be run during startup
        /// </summary>
        void Initialize(DialogProgressHandler progressHandler);

        /// <summary>
        /// Initializes the library importer directory to recursion depth 0.
        /// </summary>
        public LibraryImporterTreeViewModel InitializeImporterTree(ILibraryDirectory sourceDirectory,
                                                                   ILibraryDirectory destinationDirectory,
                                                                   string searchPattern,
                                                                   LibraryImporterConfigurationViewModel importerOptions);

        /// <summary>
        /// Loads further directories of the importer tree
        /// </summary>
        /// <param name="directory">Directory node (any)</param>
        /// <param name="sourceDirectory"></param>
        /// <param name="destinationDirectory"></param>
        /// <param name="searchPattern"></param>
        /// <param name="importerOptions"></param>
        public void LoadImporterTreeNextDepth(LibraryImporterTreeViewModel directory,
                                            ILibraryDirectory sourceDirectory,
                                            ILibraryDirectory destinationDirectory,
                                            string searchPattern,
                                            LibraryImporterConfigurationViewModel importerOptions);
    }
}
