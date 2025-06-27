using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzUrlEntityMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid MusicBrainzUrlId { get; set; }
        public Guid MusicBrainzEntityId { get; set; }
        public int MusicBrainzEntityTypeId { get; set; }
    }
}
