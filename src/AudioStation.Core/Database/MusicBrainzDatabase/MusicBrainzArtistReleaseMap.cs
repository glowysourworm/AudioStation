using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzArtistReleaseMap
    {
        // This table doesn't follow MusicBrainz Guid system
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public Guid MusicBrainzArtistId { get; set; }
        public Guid MusicBrainzReleaseId { get; set; }
    }
}
