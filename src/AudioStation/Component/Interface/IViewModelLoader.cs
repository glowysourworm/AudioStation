using AudioStation.Core.Database;
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
    }
}
