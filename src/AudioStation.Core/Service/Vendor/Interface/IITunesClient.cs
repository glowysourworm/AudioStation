using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface IITunesClient : IAudioStationService
    {
        Task<ITunesNowPlaying> SearchArtist(string artistName, string albumName);
    }
}
