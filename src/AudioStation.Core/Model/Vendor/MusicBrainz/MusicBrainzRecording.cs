using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Model.Vendor
{
    public class MusicBrainzRecording : IRecording
    {
        public IReadOnlyList<INameCredit>? ArtistCredit { get; set; }
        public PartialDate? FirstReleaseDate { get; set; }
        public IReadOnlyList<string>? Isrcs { get; set; }
        public TimeSpan? Length { get; set; }
        public IReadOnlyList<IRelease>? Releases { get; set; }
        public bool Video { get; set; }
        public IReadOnlyList<IAlias>? Aliases { get; set; }
        public string? Annotation { get; set; }
        public IRating? Rating { get; set; }
        public IRating? UserRating { get; set; }
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

        public MusicBrainzRecording()
        {

        }
    }
}
