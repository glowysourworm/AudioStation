using AudioStation.Component.Interface;
using AudioStation.ViewModels.Vendor.ITunesViewModel;

using iTunesSearch.Library;

namespace AudioStation.Component
{
    public class ITunesClient : IITunesClient
    {
        public async Task<ITunesNowPlayingViewModel> SearchArtist(string artistName, string albumName)
        {
            return null;
        }
    }
}
