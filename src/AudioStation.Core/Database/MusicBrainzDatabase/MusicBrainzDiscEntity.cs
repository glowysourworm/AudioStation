using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzDiscEntity : MusicBrainzEntityBase
    {
        public Guid MusicBrainzMediumId { get; set; }
        public Guid MusicBrainzReleaseId { get; set; }
    }
}
