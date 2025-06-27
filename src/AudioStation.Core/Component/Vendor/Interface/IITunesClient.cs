using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IITunesClient
    {
        Task<ITunesNowPlaying> SearchArtist(string artistName, string albumName);
    }
}
