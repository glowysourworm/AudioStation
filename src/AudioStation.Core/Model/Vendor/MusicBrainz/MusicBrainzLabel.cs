using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Model.Vendor.MusicBrainz
{
    public class MusicBrainzLabel : ILabel
    {
        public IArea? Area { get; }
        public string? Country { get; }
        public IReadOnlyList<string>? Ipis { get; }
        public IReadOnlyList<string>? Isnis { get; }
        public int? LabelCode { get; }
        public ILifeSpan? LifeSpan { get; }
        public IReadOnlyList<IRelease>? Releases { get; }
        public string? SortName { get; }
        public IReadOnlyList<IAlias>? Aliases { get; }
        public string? Annotation { get; }
        public string? Disambiguation { get; }
        public string? Name { get; }
        public IRating? Rating { get; }
        public IRating? UserRating { get; }
        public IReadOnlyList<IRelationship>? Relationships { get; }
        public IReadOnlyList<IGenre>? Genres { get; }
        public IReadOnlyList<ITag>? Tags { get; }
        public IReadOnlyList<IGenre>? UserGenres { get; }
        public IReadOnlyList<ITag>? UserTags { get; }
        public string? Type { get; }
        public Guid? TypeId { get; }
        public EntityType EntityType { get; }
        public Guid Id { get; }
        public IReadOnlyDictionary<string, object?>? UnhandledProperties { get; }
    }
}
