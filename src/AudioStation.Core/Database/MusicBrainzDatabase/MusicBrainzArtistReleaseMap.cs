using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzArtistReleaseMap", Schema = "public")]
    public class MusicBrainzArtistReleaseMap
    {
        // This table doesn't follow MusicBrainz Guid system
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("MusicBrainzArtist")]
        public Guid MusicBrainzArtistId { get; set; }

        [ForeignKey("MusicBrainzRelease")]
        public Guid MusicBrainzReleaseId { get; set; }

        public MusicBrainzArtistEntity MusicBrainzArtist { get; set; }
        public MusicBrainzReleaseEntity MusicBrainzRelease { get; set; }

        public MusicBrainzArtistReleaseMap() { }
    }
}
