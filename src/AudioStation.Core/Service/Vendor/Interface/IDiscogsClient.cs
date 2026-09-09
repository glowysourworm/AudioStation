using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface IDiscogsClient : IAudioStationDataService
    {
        Task<DiscogsNowPlaying> GetDiscogsNowPlaying(string artistName, string albumName);
    }
}
