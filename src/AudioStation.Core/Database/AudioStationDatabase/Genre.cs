using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Genre", Schema = "public")]
    public class Genre : AudioStationEntityBase
    {
        public string Name { get; set; }

        public Genre() { }
    }
}
