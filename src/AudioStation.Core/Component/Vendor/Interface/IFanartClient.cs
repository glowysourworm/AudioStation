using AudioStation.Core.Component.Interface;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IFanartClient : IAudioStationComponent
    {
        Task<IEnumerable<string>> GetArtistBackgrounds(string musicBrainzArtistId);
        Task<IEnumerable<string>> GetArtistImages(string musicBrainzArtistId);
    }
}
