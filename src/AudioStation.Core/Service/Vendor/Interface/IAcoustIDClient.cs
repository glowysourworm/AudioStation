using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface IAcoustIDClient : IAudioStationService
    {
        Task<IEnumerable<AcoustIDLookupResult>> IdentifyFingerprint(string fileName, int minScore);
    }
}
