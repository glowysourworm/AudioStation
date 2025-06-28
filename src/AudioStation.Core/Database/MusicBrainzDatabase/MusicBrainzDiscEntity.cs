using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzDisc", Schema = "public")]
    public class MusicBrainzDiscEntity : MusicBrainzEntityBase
    {
        [ForeignKey("MusicBrainzMedium")]
        public Guid MusicBrainzMediumId { get; set; }

        [ForeignKey("MusicBrainzRelease")]
        public Guid MusicBrainzReleaseId { get; set; }

        public MusicBrainzMediumEntity MusicBrainzMedium { get; set; }
        public MusicBrainzReleaseEntity MusicBrainzRelease { get; set; }

        public MusicBrainzDiscEntity() { }
    }
}
