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

        // This is not a hard foreign key constraint
        public Guid MusicBrainzEntityId { get; set; }

        [ForeignKey("MusicBrainzEntityType")]
        public int MusicBrainzEntityTypeId { get; set; }

        public MusicBrainzGenreEntity MusicBrainzGenre { get; set; }
        public MusicBrainzEntityType MusicBrainzEntityType { get; set; }

        public MusicBrainzGenreEntityMap() { }
    }
}
