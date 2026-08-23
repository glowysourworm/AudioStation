using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Artist", Schema = "public")]
    public class Artist : AudioStationEntityBase
    {
        public string Name { get; set; }
        public Guid? MusicBrainzArtistId { get; set; }

        public Artist() { }
    }
}
