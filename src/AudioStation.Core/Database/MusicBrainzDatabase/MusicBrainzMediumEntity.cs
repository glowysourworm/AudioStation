namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzMediumEntity : MusicBrainzEntityBase
    {
        public Guid MusicBrainzReleaseId { get; set; }
        public string? Title { get; set; }
        public string? Format { get; set; }
        public int Position { get; set; }        
        public int TrackCount { get; set; }
        public int? TrackOffset { get; set; }
    }
}
