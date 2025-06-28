using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzRecording", Schema = "public")]
    public class MusicBrainzRecordingEntity : MusicBrainzEntityBase
    {
        public string? Title { get; set; }
        public string? Disambiguation { get; set; }
        public TimeSpan? Length { get; set; }
        public string? Annotation { get; set; }

        public MusicBrainzRecordingEntity() { }
    }
}
