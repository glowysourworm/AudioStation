using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzArtistEntity : MusicBrainzEntityBase
    {
        public string? Country { get; set; }
        public string? SortName { get; set; }
        public string? Annotation { get; set; }
        public string? Disambiguation { get; set; }
        public string? Name { get; set; }
    }
}
