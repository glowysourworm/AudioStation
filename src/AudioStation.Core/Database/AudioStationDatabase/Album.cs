using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Album", Schema = "public")]
    public class Album : AudioStationEntityBase
    {
        public string Name { get; set; }
        public int DiscNumber { get; set; }
        public int DiscCount { get; set; }
        public int Year { get; set; }
        public Guid? MusicBrainzReleaseId { get; set; }

        public Album() { }
    }
}
