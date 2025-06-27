using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;

namespace AudioStation.Core.Model.Vendor
{
    public class MusicBrainzTrack : ITrack
    {
        public IReadOnlyList<INameCredit>? ArtistCredit { get; set; }
        public Guid Id { get; set; }
        public TimeSpan? Length { get; set; }
        public string? Number { get; set; }
        public int? Position { get; set; }
        public IRecording? Recording { get; set; }
        public string? Title { get; set; }
        public IReadOnlyDictionary<string, object?>? UnhandledProperties { get; set; }

        public MusicBrainzTrack()
        {

        }
    }
}
