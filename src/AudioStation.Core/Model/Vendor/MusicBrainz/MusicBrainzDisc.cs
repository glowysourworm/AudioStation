using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;

namespace AudioStation.Core.Model.Vendor
{
    public class MusicBrainzDisc : IDisc
    {
        public string Id { get; set; }
        public IReadOnlyList<int> Offsets { get; set; }
        public IReadOnlyList<IRelease>? Releases { get; set; }
        public int Sectors { get; set; }
        public IReadOnlyDictionary<string, object?>? UnhandledProperties { get; set; }


        public MusicBrainzDisc()
        {

        }
    }
}
