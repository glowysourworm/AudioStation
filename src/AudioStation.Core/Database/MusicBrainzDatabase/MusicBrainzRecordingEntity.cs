namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzRecordingEntity : MusicBrainzEntityBase
    {
        public string? Title { get; set; }
        public string? Disambiguation { get; set; }
        public TimeSpan? Length { get; set; }
        public string? Annotation { get; set; }
    }
}
