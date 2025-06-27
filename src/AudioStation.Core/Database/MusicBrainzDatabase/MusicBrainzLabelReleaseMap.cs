using System.ComponentModel.DataAnnotations.Schema;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzLabelReleaseMap
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid MusicBrainzLabelId { get; set; }
        public Guid MusicBrainzReleaseId { get; set; }
    }
}
