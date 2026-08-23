using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("AcoustIDChromaPrint", Schema = "public")]
    public class AcoustIDChromaPrint : AudioStationEntityBase
    {
        public Guid LookupId { get; set; }
        public Guid MusicBrainzRecordingId { get; set; }
        public double Score { get; set; }
        public string Fingerprint { get; set; }

        public AcoustIDChromaPrint()
        {
        }
    }
}
