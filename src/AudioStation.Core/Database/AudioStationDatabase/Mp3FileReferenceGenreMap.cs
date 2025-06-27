using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Mp3FileReferenceGenreMap", Schema = "public")]
    public class Mp3FileReferenceGenreMap : AudioStationEntityBase
    {
        [ForeignKey("Mp3FileReference")]
        public int Mp3FileReferenceId { get; set; }

        [ForeignKey("Mp3FileReferenceGenre")]
        public int Mp3FileReferenceGenreId { get; set; }

        public bool IsPrimaryGenre { get; set; }

        public Mp3FileReference Mp3FileReference { get; set; }
        public Mp3FileReferenceGenre Mp3FileReferenceGenre { get; set; }


        public Mp3FileReferenceGenreMap()
        {

        }
    }
}
