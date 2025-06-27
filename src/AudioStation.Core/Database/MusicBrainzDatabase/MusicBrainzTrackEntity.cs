namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzTrackEntity : MusicBrainzEntityBase
    {
        public Guid MusicBrainzMediumId { get; set; }
        public Guid MusicBrainzRecordingId { get; set; }
        public TimeSpan? Length { get; set; }
        public string? Number { get; set; }
        public int? Position { get; set; }
        public string? Title { get; set; }
    }
}
