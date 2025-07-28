using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryLoaderViewModels.Import;
using AudioStation.ViewModels.LibraryViewModels;

using static AudioStation.EventHandler.DialogEventHandlers;

namespace AudioStation.Component.Interface
{
    public interface IViewModelLoader
    {
        /// <summary>
        /// Loads a collection of ArtistViewModel ordered by name; and translates them into 
        /// one for the ViewModel namespace.
        /// </summary>
        IEnumerable<ArtistViewModel> LoadArtists(DialogProgressHandler progressHandler);

        /// <summary>
        /// Loads a collection of GenreViewModel entities ordered by name from the database.
        /// </summary>
        /// <returns></returns>
        IEnumerable<GenreViewModel> LoadGenres(DialogProgressHandler progressHandler);

        /// <summary>
        /// Loads a collection of AlbumViewModel entities ordered by name from the database.
        /// </summary>
        /// <returns></returns>
        IEnumerable<AlbumViewModel> LoadAlbums(DialogProgressHandler progressHandler);

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

        /// <summary>
        /// Loads a import directory tree (recursively) and returns the base directory
        /// </summary>
        Task<LibraryLoaderImportTreeViewModel?> LoadImportFiles(LibraryLoaderImportOptionsViewModel options,
                                                                DialogProgressHandler progressHandler);
    }
}
