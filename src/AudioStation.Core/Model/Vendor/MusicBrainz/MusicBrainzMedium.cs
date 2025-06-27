using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;

namespace AudioStation.Core.Model.Vendor
{
    public class MusicBrainzMedium : IMedium
    {
        public IReadOnlyList<ITrack>? DataTracks { get; set; }
        public IReadOnlyList<IDisc>? Discs { get; set; }
        public string? Format { get; set; }
        public Guid? FormatId { get; set; }
        public int Position { get; set; }
        public ITrack? Pregap { get; set; }
        public string? Title { get; set; }
        public int TrackCount { get; set; }
        public int? TrackOffset { get; set; }
        public IReadOnlyList<ITrack>? Tracks { get; set; }
        public IReadOnlyDictionary<string, object?>? UnhandledProperties { get; set; }


        public MusicBrainzMedium()
        {

        }
    }
}
