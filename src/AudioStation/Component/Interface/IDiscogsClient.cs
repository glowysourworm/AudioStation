using AudioStation.ViewModels.Vendor.DiscogsViewModel;

namespace AudioStation.Component.Interface
{
    public interface IDiscogsClient
    {
        Task<DiscogsNowPlayingViewModel> GetDiscogsNowPlaying(string artistName, string albumName);
    }
}
