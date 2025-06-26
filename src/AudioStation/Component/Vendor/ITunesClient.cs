using AudioStation.Component.Vendor.Interface;
using AudioStation.ViewModels.Vendor.ITunesViewModel;

using iTunesSearch.Library;

namespace AudioStation.Component.Vendor
{
    public class ITunesClient : IITunesClient
    {
        public async Task<ITunesNowPlayingViewModel> SearchArtist(string artistName, string albumName)
        {
            return null;
        }
    }
}
