using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("AcoustIDLookupResult", Schema = "public")]
    public class AcoustIDLookupResult : AudioStationEntityBase
    {
        [ForeignKey("AcoustIDChromaPrint")]
        public int AcoustIDChromaPrintId { get; set; }

        // Vendor result (from service)
        public Guid LookupId { get; set; }
        public Guid MusicBrainzRecordingId { get; set; }
        public double Score { get; set; }
        public string Fingerprint { get; set; }

        public AcoustIDChromaPrint AcoustIDChromaPrint { get; set; }

        public AcoustIDLookupResult()
        {
        }
    }
}
