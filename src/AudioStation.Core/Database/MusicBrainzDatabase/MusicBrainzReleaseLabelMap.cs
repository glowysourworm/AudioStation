using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzReleaseLabelMap
    {
        public Guid MusicBrainzReleaseId { get; set; }
        public Guid MusicBrainzLabelId { get; set; }
    }
}
