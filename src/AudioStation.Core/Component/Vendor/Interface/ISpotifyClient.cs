using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface ISpotifyClient
    {
        Task<SpotifyNowPlaying?> CreateNowPlaying(string artistName, string albumName);
    }
}
