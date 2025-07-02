using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AcoustID.Web;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IAcoustIDClient
    {
        Task<IEnumerable<LookupResult>> IdentifyFingerprint(string fileName, int minScore);
    }
}
