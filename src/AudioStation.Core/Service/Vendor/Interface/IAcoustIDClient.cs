using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface IAcoustIDClient : IAudioStationDataService
    {
        IEnumerable<AcoustIDLookupResult> IdentifyFingerprint(string fileName);
        Task<IEnumerable<AcoustIDLookupResult>> IdentifyFingerprintAsync(string fileName);
    }
}
