namespace AudioStation.Component.Bandcamp.Interface
{
    public interface IBandcampClient
    {
        Task Download(string endpoint);
    }
}
