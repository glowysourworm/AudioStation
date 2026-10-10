using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("FileType", Schema = "public")]
    public class FileType : AudioStationEnumEntityBase
    {
        public FileType() { }
    }
}
