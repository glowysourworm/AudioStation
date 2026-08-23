using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Component.Interface
{
    public interface IComponentViewModelLoader
    {
        /// <summary>
        /// Initialization of the view model - this should be run during startup
        /// </summary>
        Task Initialize(DialogProgressHandler progressHandler);

        /// <summary>
        /// Runs AcoustID service on the staged files of the library importer
        /// </summary>
        Task LibraryImporter_RunAcoustID();

        /// <summary>
        /// Runs Music Brainz service on the staged files of the library importer
        /// </summary>
        Task LibraryImporter_RunMusicBrainz();

        /// <summary>
        /// Runs import on staged files of the library importer
        /// </summary>
        Task LibraryImporter_RunImport();

        /// <summary>
        /// Loads a collection of LibraryEntryViewModel ordered by ID; and translates the PageResult into 
        /// one for the ViewModel namespace.
        /// </summary>
        PageResult<LibraryEntryViewModel> LoadEntryPage(PageRequest<Track, int> request);

        /// <summary>
        /// Converts any non-mp3 files to mp3 files and puts them in a special staging folder to be imported.
        /// </summary>
        Task ConvertFiles(IEnumerable<string> convertibleFiles, Action<double, string> progressCallback);
    }
}
