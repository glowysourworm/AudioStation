using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IAudioDBClient
    {
        Task<AudioDBArtist> SearchArtist(string artistName);
    }
}
