using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface ILastFmClient
    {
        Task<LastFmNowPlaying> GetNowPlayingInfo(string artist, string album);
    }
}
