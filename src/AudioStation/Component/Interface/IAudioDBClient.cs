using AudioStation.ViewModels.Vendor.AudioDBViewModel;

namespace AudioStation.Component.Interface
{
    public interface IAudioDBClient
    {
        Task<AudioDBArtistViewModel> SearchArtist(string artistName);

        Task<AudioDBNowPlayingViewModel> CreateNowPlaying(int artistId, int albumId);
    }
}
