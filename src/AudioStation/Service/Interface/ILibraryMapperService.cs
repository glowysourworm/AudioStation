using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;

namespace AudioStation.Service.Interface
{
    /// <summary>
    /// Maps library entities to the view model namespace
    /// </summary>
    public interface ILibraryMapperService : IAudioStationService
    {
        public AlbumViewModel MapAlbum(Artist primaryArtist, Album albumEntity, IEnumerable<Track> tracks);
        public TrackViewModel MapTrack(Track track);
    }
}
