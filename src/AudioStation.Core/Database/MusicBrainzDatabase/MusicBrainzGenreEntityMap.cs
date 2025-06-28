using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzGenreEntityMap", Schema = "public")]
    public class MusicBrainzGenreEntityMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("MusicBrainzGenre")]
        public Guid MusicBrainzGenreId { get; set; }

        [ForeignKey("MusicBrainzEntity")]
        public Guid MusicBrainzEntityId { get; set; }

        [ForeignKey("MusicBrainzEntityType")]
        public int MusicBrainzEntityTypeId { get; set; }

        public MusicBrainzGenreEntity MusicBrainzGenre { get; set; }
        public MusicBrainzEntityBase MusicBrainzEntity { get; set; }
        public MusicBrainzEntityType MusicBrainzEntityType { get; set; }

        public MusicBrainzGenreEntityMap() { }
    }
}
