namespace AudioStation.Component.Vendor.Bandcamp.Interface
{
    public interface IBandcampClient
    {
        Task Download(string endpoint);
    }
}
