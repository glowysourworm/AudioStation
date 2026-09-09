using AudioStation.Model;
using AudioStation.ViewModels.ComponentViewModels.LibraryViewModels;

namespace AudioStation.Service.Interface
{
    public interface INowPlayingService : IAudioStationService
    {
        Task<NowPlayingData> LoadPlaylist(ArtistViewModel artist, AlbumViewModel album, TrackViewModel startTrack);
    }
}
