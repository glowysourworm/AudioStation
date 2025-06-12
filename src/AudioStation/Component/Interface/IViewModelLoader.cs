using AudioStation.Core.Database;
using AudioStation.Core.Model;
using AudioStation.ViewModel.LibraryViewModels;

namespace AudioStation.Component.Interface
{
    public interface IViewModelLoader
    {
        /// <summary>
        /// Loads a collection of ArtistViewModel ordered by name; and translates the PageResult into 
        /// one for the ViewModel namespace.
        /// </summary>
        PageResult<ArtistViewModel> LoadArtistPage(PageRequest<Mp3FileReferenceArtist, string> request);
    }
}
