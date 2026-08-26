using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("AlbumFileReferenceMap", Schema = "public")]
    public class AlbumFileReferenceMap : AudioStationEntityBase
    {
        [ForeignKey("Album")]
        public int AlbumId { get; set; }

        [ForeignKey("FileReference")]
        public int FileReferenceId { get; set; }

        [ForeignKey("FileType")]
        public int FileTypeId { get; set; }

        public Album Album { get; set; }
        public FileReference FileReference { get; set; }
        public FileType FileType { get; set; }

        public AlbumFileReferenceMap() { }
    }
}
