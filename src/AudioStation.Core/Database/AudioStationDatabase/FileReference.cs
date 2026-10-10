using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("FileReference", Schema = "public")]
    public class FileReference : AudioStationEntityBase
    {
        [ForeignKey("FileType")]
        public Guid FileTypeId { get; set; }

        public string FileName { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateAdded { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateLastModified { get; set; }

        public FileType FileType { get; set; }

        public FileReference() { }
    }
}
