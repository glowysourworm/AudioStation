using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Service.Vendor.Bandcamp.Interface
{
    public interface IBandcampClient : IAudioStationDataService
    {
        Task Download(string endpoint);
    }
}
