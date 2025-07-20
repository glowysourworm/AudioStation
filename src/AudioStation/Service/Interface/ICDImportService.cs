namespace AudioStation.Service.Interface
{
    public interface ICDImportService
    {


        Task ImportTrack(int trackNumber, string artist, string album, int discNumber, int discCount, Action<double> progressCallback);
    }
}
