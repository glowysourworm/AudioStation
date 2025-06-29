using System.ComponentModel.DataAnnotations.Schema;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzLabel", Schema = "public")]
    public class MusicBrainzLabelEntity : MusicBrainzEntityBase
    {
        public int? LabelCode { get; set; }
        public string? Name { get; set; }
        public string? SortName { get; set; }
        public string? Disambiguation { get; set; }                
        public string? Type { get; set; }
        public string? Country { get; set; }
        public string? Annotation { get; set; }

        public MusicBrainzLabelEntity() { }
    }
}
