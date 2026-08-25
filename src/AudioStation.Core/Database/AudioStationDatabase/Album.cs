using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Album", Schema = "public")]
    public class Album : AudioStationEntityBase
    {
        public string Name { get; set; }
        public int? MediaNumber { get; set; }
        public int? MediaCount { get; set; }
        public string? MediaFormat { get; set; }             // See MediaFormats.cs
        public int? Year { get; set; }
        public Guid? MusicBrainzReleaseId { get; set; }

        public Album() { }
    }
}
