using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzMedium", Schema = "public")]
    public class MusicBrainzMediumEntity : MusicBrainzEntityBase
    {
        [ForeignKey("MusicBrainzRelease")]
        public Guid MusicBrainzReleaseId { get; set; }
        public string? Title { get; set; }
        public string? Format { get; set; }
        public int Position { get; set; }        
        public int TrackCount { get; set; }
        public int? TrackOffset { get; set; }

        public MusicBrainzReleaseEntity MusicBrainzRelease { get; set; }

        public MusicBrainzMediumEntity() { }
    }
}
