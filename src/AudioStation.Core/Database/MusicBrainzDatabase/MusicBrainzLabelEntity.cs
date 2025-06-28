using System.ComponentModel.DataAnnotations.Schema;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.EntityFrameworkCore;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    [PrimaryKey("Id")]
    [Table("MusicBrainzLabel", Schema = "public")]
    public class MusicBrainzLabelEntity : MusicBrainzEntityBase
    {
        public int? LabelCode { get; }
        public string? Name { get; }
        public string? SortName { get; }
        public string? Disambiguation { get; }                
        public string? Type { get; }
        public string? Country { get; }
        public string? Annotation { get; }

        public MusicBrainzLabelEntity() { }
    }
}
