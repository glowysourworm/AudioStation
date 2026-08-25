using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("TagSmallVendorMap", Schema = "public")]
    public class TagSmallVendorMap : AudioStationEntityBase
    {
        [ForeignKey("TagSmall")]
        public int TagSmallId { get; set; }

        [ForeignKey("Vendor")]
        public int VendorId { get; set; }

        public Guid? MusicBrainzRecordingId { get; set; }           // Add columns per vendor as needed

        public TagSmall TagSmall { get; set; }
        public Vendor Vendor { get; set; }

        public TagSmallVendorMap() { }
    }
}
