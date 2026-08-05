using AudioStation.Core.Component.Interface;

namespace AudioStation.Core.Component.Vendor.Bandcamp.Interface
{
    public interface IBandcampClient : IAudioStationComponent
    {
        Task Download(string endpoint);
    }
}
