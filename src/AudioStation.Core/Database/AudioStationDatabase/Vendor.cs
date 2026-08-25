using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.AudioStationDatabase
{
    [PrimaryKey("Id")]
    [Table("Vendor", Schema = "public")]
    public class Vendor : AudioStationEntityBase
    {
        public string VendorName { get; set; }

        public Vendor()
        {

        }
    }
}
