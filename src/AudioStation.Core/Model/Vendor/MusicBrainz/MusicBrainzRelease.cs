
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Model.Vendor
{
    public class MusicBrainzRelease : IRelease
    {
        public IReadOnlyList<INameCredit>? ArtistCredit { get; set; }
        public string? Asin { get; set; }
        public string? Barcode { get; set; }
        public IReadOnlyList<ICollection>? Collections { get; set; }
        public string? Country { get; set; }
        public ICoverArtArchive? CoverArtArchive { get; set; }
        public PartialDate? Date { get; set; }
        public IReadOnlyList<ILabelInfo>? LabelInfo { get; set; }
        public IReadOnlyList<IMedium>? Media { get; set; }
        public string? Packaging { get; set; }
        public Guid? PackagingId { get; set; }
        public string? Quality { get; set; }
        public IReadOnlyList<IReleaseEvent>? ReleaseEvents { get; set; }
        public IReleaseGroup? ReleaseGroup { get; set; }
        public string? Status { get; set; }
        public Guid? StatusId { get; set; }
        public ITextRepresentation? TextRepresentation { get; set; }
        public IReadOnlyList<IAlias>? Aliases { get; set; }
        public string? Annotation { get; set; }
        public IReadOnlyList<IRelationship>? Relationships { get; set; }
        public IReadOnlyList<IGenre>? Genres { get; set; }
        public IReadOnlyList<ITag>? Tags { get; set; }
        public IReadOnlyList<IGenre>? UserGenres { get; set; }
        public IReadOnlyList<ITag>? UserTags { get; set; }
        public string? Disambiguation { get; set; }
        public string? Title { get; set; }
        public EntityType EntityType { get; set; }
        public Guid Id { get; set; }
        public IReadOnlyDictionary<string, object?>? UnhandledProperties { get; set; }

        public MusicBrainzRelease()
        {
        }
    }
}
