using AudioStation.ViewModels.Vendor.SpotifyViewModel;

namespace AudioStation.Component.Vendor.Interface
{
    public interface ISpotifyClient
    {
        Task<SpotifyNowPlayingViewModel?> CreateNowPlaying(string artistName, string albumName);
    }
}
