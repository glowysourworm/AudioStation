using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface IFanartClient : IAudioStationService
    {
        Task<IEnumerable<string>> GetArtistBackgrounds(string musicBrainzArtistId);
        Task<IEnumerable<string>> GetArtistImages(string musicBrainzArtistId);
    }
}
