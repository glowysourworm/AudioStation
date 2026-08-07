using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface IAudioDBClient
    {
        Task<AudioDBArtist> SearchArtist(string artistName);
    }
}
