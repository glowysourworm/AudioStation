using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzEntityType
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
