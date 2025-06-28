using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzLabelReleaseMap", Schema = "public")]
    public class MusicBrainzLabelReleaseMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("MusicBrainzLabel")]
        public Guid MusicBrainzLabelId { get; set; }

        [ForeignKey("MusicBrainzRelease")]
        public Guid MusicBrainzReleaseId { get; set; }

        public MusicBrainzLabelEntity MusicBrainzLabel { get; set; }
        public MusicBrainzReleaseEntity MusicBrainzRelease { get; set; }

        public MusicBrainzLabelReleaseMap() { }
    }
}
