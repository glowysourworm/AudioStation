using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AcoustID.Web;

using AudioStation.Core.Service.Interface;

namespace AudioStation.Core.Service.Vendor.Interface
{
    public interface IAcoustIDClient : IAudioStationService
    {
        Task<IEnumerable<LookupResult>> IdentifyFingerprint(string fileName, int minScore);
    }
}
