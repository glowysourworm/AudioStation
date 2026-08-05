using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface ISpotifyClient : IAudioStationComponent
    {
        Task<SpotifyNowPlaying?> CreateNowPlaying(string artistName, string albumName);
    }
}
