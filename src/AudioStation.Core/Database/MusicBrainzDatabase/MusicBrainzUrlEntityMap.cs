using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzUrlEntityMap", Schema = "public")]
    public class MusicBrainzUrlEntityMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("MusicBrainzUrl")]
        public Guid MusicBrainzUrlId { get; set; }

        [ForeignKey("MusicBrainzEntity")]
        public Guid MusicBrainzEntityId { get; set; }

        [ForeignKey("MusicBrainzEntityType")]
        public int MusicBrainzEntityTypeId { get; set; }

        public MusicBrainzUrlEntity MusicBrainzUrl { get; set; }
        public MusicBrainzEntityBase MusicBrainzEntity { get; set; }
        public MusicBrainzEntityType MusicBrainzEntityType { get; set; }

        public MusicBrainzUrlEntityMap() { }
    }
}
