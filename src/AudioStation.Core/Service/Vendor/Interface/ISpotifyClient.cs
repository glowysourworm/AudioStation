using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface ISpotifyClient : IAudioStationService
    {
        Task<SpotifyNowPlaying?> CreateNowPlaying(string artistName, string albumName);
    }
}
