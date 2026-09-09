using AudioStation.Core.Service.Interface;

namespace AudioStation.Service.Interface
{
    public interface ICDImportService : IAudioStationDataService
    {


        Task ImportTrack(int trackNumber, string artist, string album, int discNumber, int discCount, Action<double> progressCallback);
    }
}
