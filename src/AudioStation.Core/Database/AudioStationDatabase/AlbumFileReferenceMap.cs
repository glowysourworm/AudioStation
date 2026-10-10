using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("AlbumFileReferenceMap", Schema = "public")]
    public class AlbumFileReferenceMap : AudioStationEntityBase
    {
        [ForeignKey("Album")]
        public Guid AlbumId { get; set; }

        [ForeignKey("FileReference")]
        public Guid FileReferenceId { get; set; }

        [ForeignKey("FileType")]
        public Guid FileTypeId { get; set; }

        public Album Album { get; set; }
        public FileReference FileReference { get; set; }
        public FileType FileType { get; set; }

        public AlbumFileReferenceMap() { }
    }
}
