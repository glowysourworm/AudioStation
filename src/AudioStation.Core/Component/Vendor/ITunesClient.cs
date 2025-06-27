using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor
{
    public class ITunesClient : IITunesClient
    {
        public async Task<ITunesNowPlaying> SearchArtist(string artistName, string albumName)
        {
            return null;
        }
    }
}
