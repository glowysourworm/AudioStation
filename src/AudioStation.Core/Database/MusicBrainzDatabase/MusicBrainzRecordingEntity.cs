namespace AudioStation.Core.Database.MusicBrainzDatabase
{
    public class MusicBrainzRecordingEntity : MusicBrainzEntityBase
    {
        public TimeSpan? Length { get; set; }
        public bool Video { get; set; }
        public string? Annotation { get; set; }
        public string? Disambiguation { get; set; }
        public string? Title { get; set; }
    }
}
