using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("ArtistFileReferenceMap", Schema = "public")]
    public class ArtistFileReferenceMap : AudioStationEntityBase
    {
        [ForeignKey("Artist")]
        public int ArtistId { get; set; }

        [ForeignKey("FileReference")]
        public int FileReferenceId { get; set; }

        [ForeignKey("FileType")]
        public int FileTypeId { get; set; }

        public Artist Artist { get; set; }
        public FileReference FileReference { get; set; }
        public FileType FileType { get; set; }

        public ArtistFileReferenceMap() { }
    }
}
