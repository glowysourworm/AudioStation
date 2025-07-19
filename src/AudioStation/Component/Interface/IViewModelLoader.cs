using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.Component.Interface
{
    public interface IViewModelLoader
    {
        /// <summary>
        /// Loads a collection of ArtistViewModel ordered by name; and translates the PageResult into 
        /// one for the ViewModel namespace.
        /// </summary>
        PageResult<ArtistViewModel> LoadArtistPage(PageRequest<Mp3FileReferenceArtist, string> request);

        /// <summary>
        /// Loads a collection of LibraryEntryViewModel ordered by ID; and translates the PageResult into 
        /// one for the ViewModel namespace.
        /// </summary>
        PageResult<LibraryEntryViewModel> LoadEntryPage(PageRequest<Mp3FileReference, int> request);

        /// <summary>
        /// Loads a collection of non-converted files from the library base folder. These would also be
        /// non-imported.
        /// </summary>
        IEnumerable<string> LoadNonConvertedFiles();

        /// <summary>
        /// Converts any non-mp3 files to mp3 files and puts them in a special staging folder to be imported.
        /// </summary>
        Task ConvertFiles(IEnumerable<string> convertibleFiles, Action<double, string> progressCallback); 
    }
}
