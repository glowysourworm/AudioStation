
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Model.Vendor
{
    public class MusicBrainzRelationship : IRelationship
    {
        public IArea? Area { get; set; }
        public IArtist? Artist { get; set; }
        public IReadOnlyList<string>? Attributes { get; set; }
        public IReadOnlyDictionary<string, string>? AttributeCredits { get; set; }
        public IReadOnlyDictionary<string, Guid>? AttributeIds { get; set; }
        public IReadOnlyDictionary<string, string>? AttributeValues { get; set; }
        public PartialDate? Begin { get; set; }
        public string? Direction { get; set; }
        public PartialDate? End { get; set; }
        public bool Ended { get; set; }
        public IEvent? Event { get; set; }
        public IInstrument? Instrument { get; set; }
        public ILabel? Label { get; set; }
        public int? OrderingKey { get; set; }
        public IPlace? Place { get; set; }
        public IRecording? Recording { get; set; }
        public IRelease? Release { get; set; }
        public IReleaseGroup? ReleaseGroup { get; set; }
        public ISeries? Series { get; set; }
        public string? SourceCredit { get; set; }
        public IRelatableEntity? Target { get; set; }
        public string? TargetCredit { get; set; }
        public Guid? TargetId { get; set; }
        public EntityType? TargetType { get; set; }
        public string? Type { get; set; }
        public Guid? TypeId { get; set; }
        public IUrl? Url { get; set; }
        public IWork? Work { get; set; }
        public IReadOnlyDictionary<string, object?>? UnhandledProperties { get; set; }

        public MusicBrainzRelationship()
        {

        }
    }
}
