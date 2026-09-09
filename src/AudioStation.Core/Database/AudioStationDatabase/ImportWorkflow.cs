using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("ImportWorkflow", Schema = "public")]
    public class ImportWorkflow : AudioStationEntityBase
    {
        public string Directory { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public int StepNumber { get; set; }
    }
}
