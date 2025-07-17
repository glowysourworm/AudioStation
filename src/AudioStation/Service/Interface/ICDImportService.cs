namespace AudioStation.Service.Interface
{
    public interface ICDImportService
    {
        Task ImportTrack(int trackNumber, string artist, string album, Action<double> progressCallback);
    }
}
