using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzLabelEntity : MusicBrainzEntityBase
    {
        public int? LabelCode { get; }
        public string? Name { get; }
        public string? SortName { get; }
        public string? Disambiguation { get; }                
        public string? Type { get; }
        public string? Country { get; }
        public string? Annotation { get; }
    }
}
