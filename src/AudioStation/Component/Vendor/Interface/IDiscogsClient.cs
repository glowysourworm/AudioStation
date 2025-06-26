using AudioStation.ViewModels.Vendor.DiscogsViewModel;

namespace AudioStation.Component.Vendor.Interface
{
    public interface IDiscogsClient
    {
        Task<DiscogsNowPlayingViewModel> GetDiscogsNowPlaying(string artistName, string albumName);
    }
}
