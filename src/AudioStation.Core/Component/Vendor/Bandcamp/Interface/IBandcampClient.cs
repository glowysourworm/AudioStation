namespace AudioStation.Core.Component.Vendor.Bandcamp.Interface
{
    public interface IBandcampClient
    {
        Task Download(string endpoint);
    }
}
