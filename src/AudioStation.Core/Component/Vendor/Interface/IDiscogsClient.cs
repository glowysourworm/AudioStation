using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IDiscogsClient
    {
        Task<DiscogsNowPlaying> GetDiscogsNowPlaying(string artistName, string albumName);
    }
}
