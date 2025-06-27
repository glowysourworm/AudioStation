using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Model.Vendor
{
    public class MusicBrainzUrl : IUrl
    {
        public Uri? Resource { get; set; }
        public IReadOnlyList<IRelationship>? Relationships { get; set; }
        public EntityType EntityType { get; set; }
        public Guid Id { get; set; }
        public IReadOnlyDictionary<string, object?>? UnhandledProperties { get; set; }

        public MusicBrainzUrl()
        {

        }
    }
}
