using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Album", Schema = "public")]
    public class Album : AudioStationEntityBase
    {
        [ForeignKey("Artist")]
        public Guid ArtistId { get; set; }

        public string Name { get; set; }
        public int MediaCount { get; set; }
        public int TrackCount { get; set; }
        public string MediaFormat { get; set; }             // See MediaFormats.cs
        public int Year { get; set; }

        public Artist Artist { get; set; }

        public Album() { }
    }
}
