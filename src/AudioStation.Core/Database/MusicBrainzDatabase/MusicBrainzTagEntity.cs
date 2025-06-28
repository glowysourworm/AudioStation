using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzTag", Schema = "public")]
    public class MusicBrainzTagEntity : MusicBrainzEntityBase
    {
        public string Name { get; set; }

        public MusicBrainzTagEntity() { }
    }
}
