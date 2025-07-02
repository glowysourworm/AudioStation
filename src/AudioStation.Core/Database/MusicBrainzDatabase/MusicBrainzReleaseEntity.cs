using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzRelease", Schema = "public")]
    public class MusicBrainzReleaseEntity : MusicBrainzEntityBase
    {
        public string? Asin { get; set; }
        public string? Barcode { get; set; }
        public string? Country { get; set; }
        public DateTime? Date { get; set; }
        public string? Packaging { get; set; }
        public string? Quality { get; set; }
        public string? Status { get; set; }
        public string? Annotation { get; set; }
        public string? Disambiguation { get; set; }
        public string? Title { get; set; }

        public List<MusicBrainzMediumEntity> Media { get; set; }

        public MusicBrainzReleaseEntity() 
        {
            this.Media = new List<MusicBrainzMediumEntity>();
        }
    }
}
