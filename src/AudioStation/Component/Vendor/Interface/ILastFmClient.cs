using AudioStation.ViewModels.Vendor.LastFmViewModel;

namespace AudioStation.Component.Vendor.Interface
{
    public interface ILastFmClient
    {
        Task<LastFmNowPlayingViewModel> GetNowPlayingInfo(string artist, string album);
    }
}
