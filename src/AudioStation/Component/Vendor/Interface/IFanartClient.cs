namespace AudioStation.Component.Vendor.Interface
{
    public interface IFanartClient
    {
        Task<IEnumerable<string>> GetArtistBackgrounds(string musicBrainzArtistId);
        Task<IEnumerable<string>> GetArtistImages(string musicBrainzArtistId);
    }
}
