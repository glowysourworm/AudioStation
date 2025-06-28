using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzUrl", Schema = "public")]
    public class MusicBrainzUrlEntity : MusicBrainzEntityBase
    {
        public string Url { get; set; }

        public MusicBrainzUrlEntity() { }
    }
}
