using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzArtistTrackMap", Schema = "public")]
    public class MusicBrainzArtistTrackMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("MusicBrainzArtist")]
        public Guid MusicBrainzArtistId { get; set; }

        [ForeignKey("MusicBrainzTrack")]
        public Guid MusicBrainzTrackId { get; set; }

        public MusicBrainzArtistEntity MusicBrainzArtist { get; set; }
        public MusicBrainzTrackEntity MusicBrainzTrack { get; set; }

        public MusicBrainzArtistTrackMap() { }
    }
}
