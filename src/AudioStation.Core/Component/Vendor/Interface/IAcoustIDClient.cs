using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AcoustID.Web;

using AudioStation.Core.Component.Interface;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IAcoustIDClient : IAudioStationComponent
    {
        Task<IEnumerable<LookupResult>> IdentifyFingerprint(string fileName, int minScore);
    }
}
