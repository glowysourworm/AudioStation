using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("FileType", Schema = "public")]
    public class FileType : AudioStationEntityBase
    {
        public string Name { get; set; }

        public FileType() { }
    }
}
