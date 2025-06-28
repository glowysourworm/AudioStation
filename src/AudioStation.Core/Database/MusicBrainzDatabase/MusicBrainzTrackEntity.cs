using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzTrack", Schema = "public")]
    public class MusicBrainzTrackEntity : MusicBrainzEntityBase
    {
        [ForeignKey("MusicBrainzMedium")]
        public Guid MusicBrainzMediumId { get; set; }

        [ForeignKey("MusicBrainzRecording")]
        public Guid MusicBrainzRecordingId { get; set; }

        public string? Title { get; set; }
        public string? Number { get; set; }
        public int? Position { get; set; }
        public TimeSpan? Length { get; set; }

        public MusicBrainzMediumEntity MusicBrainzMedium { get; set; }
        public MusicBrainzRecordingEntity MusicBrainzRecording { get; set; }

        public MusicBrainzTrackEntity() { }
    }
}
