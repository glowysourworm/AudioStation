using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("FileReference", Schema = "public")]
    public class FileReference : AudioStationEntityBase
    {
        public string FileName { get; set; }


        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime LastModified { get; set; }
        public bool IsFileAvailable { get; set; }
        public bool IsFileCorrupt { get; set; }
        public bool IsFileLoadError { get; set; }
        public string? FileErrorMessage { get; set; }
        public string? FileCorruptMessage { get; set; }
        public int CRC32 { get; set; }

        public FileReference() { }
    }
}
