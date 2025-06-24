using AudioStation.ViewModels.Vendor.LastFmViewModel;

namespace AudioStation.Component.Interface
{
    public interface ILastFmClient
    {
        Task<LastFmNowPlayingViewModel> GetNowPlayingInfo(string artist, string album);
    }
}
