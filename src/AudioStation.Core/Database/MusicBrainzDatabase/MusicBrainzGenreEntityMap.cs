using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzGenreEntityMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid MusicBrainzGenreId { get; set; }
        public Guid MusicBrainzEntityId { get; set; }
        public int MusicBrainzEntityTypeId { get; set; }
    }
}
