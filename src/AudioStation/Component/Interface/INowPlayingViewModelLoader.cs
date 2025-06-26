using AudioStation.Component.Model;
using AudioStation.ViewModels.LibraryViewModels;

namespace AudioStation.Component.Interface
{
    public interface INowPlayingViewModelLoader
    {
        Task<NowPlayingData> LoadPlaylist(ArtistViewModel artist, AlbumViewModel album, LibraryEntryViewModel startTrack);
    }
}
