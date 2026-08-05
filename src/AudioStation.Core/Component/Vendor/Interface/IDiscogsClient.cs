using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IDiscogsClient : IAudioStationComponent
    {
        Task<DiscogsNowPlaying> GetDiscogsNowPlaying(string artistName, string albumName);
    }
}
