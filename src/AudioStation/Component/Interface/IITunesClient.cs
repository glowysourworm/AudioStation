using AudioStation.ViewModels.Vendor.ITunesViewModel;

namespace AudioStation.Component.Interface
{
    public interface IITunesClient
    {
        Task<ITunesNowPlayingViewModel> SearchArtist(string artistName, string albumName);
    }
}
