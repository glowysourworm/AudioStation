using System.ComponentModel.DataAnnotations.Schema;

using AudioStation.Core.Model.Interface;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("VendorTagSmall", Schema = "public")]
    public class VendorTagSmall : AudioStationEntityBase, ITagSmall
    {
        [ForeignKey("Vendor")]
        public int VendorId { get; set; }

        // This will be filled in by the 3rd party service. It must be enough to query the data below.
        public Guid VendorRecordId { get; set; }

        public string? AlbumArtist { get; set; }
        public string? Album { get; set; }
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public int TrackNumber { get; set; }
        public int TrackTotal { get; set; }
        public int DiscNumber { get; set; }
        public int DiscTotal { get; set; }

        public Vendor Vendor { get; set; }

        public VendorTagSmall()
        {
        }
    }
}
