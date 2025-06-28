using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzArtistRecordingMap", Schema = "public")]
    public class MusicBrainzArtistRecordingMap
    {
        // This table doesn't follow MusicBrainz Guid system
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("MusicBrainzArtist")]
        public Guid MusicBrainzArtistId { get; set; }

        [ForeignKey("MusicBrainzRecording")]
        public Guid MusicBrainzRecordingId { get; set; }

        public MusicBrainzArtistEntity MusicBrainzArtist { get; set; }
        public MusicBrainzRecordingEntity MusicBrainzRecording { get; set; }

        public MusicBrainzArtistRecordingMap() { }
    }
}
