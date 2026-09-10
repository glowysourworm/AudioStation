using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("ImportWorkflow", Schema = "public")]
    public class ImportWorkflow : AudioStationEntityBase
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public byte[]? ConfigurationJson { get; set; }
    }
}
