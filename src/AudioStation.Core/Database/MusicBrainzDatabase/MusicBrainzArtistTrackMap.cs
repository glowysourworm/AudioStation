using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzArtistTrackMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid MusicBrainzArtistId { get; set; }
        public Guid MusicBrainzTrackId { get; set; }
    }
}
