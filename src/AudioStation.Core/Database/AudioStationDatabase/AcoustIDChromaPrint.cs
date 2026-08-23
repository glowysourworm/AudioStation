using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("AcoustIDChromaPrint", Schema = "public")]
    public class AcoustIDChromaPrint : AudioStationEntityBase
    {
        public string Fingerprint { get; set; }

        public AcoustIDChromaPrint()
        {
        }
    }
}
