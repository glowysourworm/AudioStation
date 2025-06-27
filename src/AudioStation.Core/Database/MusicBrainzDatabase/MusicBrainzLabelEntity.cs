using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzLabelEntity : MusicBrainzEntityBase
    {
        public string? Country { get; }
        public int? LabelCode { get; }
        public IReadOnlyList<IRelease>? Releases { get; }
        public string? SortName { get; }
        public string? Annotation { get; }
        public string? Disambiguation { get; }
        public string? Name { get; }
        public string? Type { get; }
    }
}
