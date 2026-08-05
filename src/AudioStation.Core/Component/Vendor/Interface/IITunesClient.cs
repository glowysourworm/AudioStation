using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IITunesClient : IAudioStationComponent
    {
        Task<ITunesNowPlaying> SearchArtist(string artistName, string albumName);
    }
}
