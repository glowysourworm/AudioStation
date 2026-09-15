using System.ComponentModel.DataAnnotations.Schema;

using AudioStation.Core.Database.AudioStationDatabase.Interface;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("AcoustIDLookupResult", Schema = "public")]
    public class AcoustIDLookupResult : AudioStationEntityBase, IAcoustIDLookupResult
    {
        /// <summary>
        /// Reference file name from the lookup
        /// </summary>
        public string FileName { get; set; }
        public string Message { get; set; }

        // Vendor result (from service)
        public Guid LookupId { get; set; }
        public Guid MusicBrainzRecordingId { get; set; }
        public double Score { get; set; }

        public AcoustIDLookupResult()
        {
        }
    }
}
