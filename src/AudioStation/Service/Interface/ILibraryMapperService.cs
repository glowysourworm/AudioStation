using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.Service.Interface
{
    /// <summary>
    /// Maps library entities to the view model namespace
    /// </summary>
    public interface ILibraryMapperService : IAudioStationService
    {
        AlbumViewModel MapAlbum(Artist primaryArtist, Album albumEntity, IEnumerable<Track> tracks);
        IEnumerable<MediaViewModel> MapMedia(Album albumEntity, IEnumerable<Track> tracks);
        TrackViewModel MapTrack(Track track);
    }
}
