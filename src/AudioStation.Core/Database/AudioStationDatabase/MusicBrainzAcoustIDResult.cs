namespace AudioStation.Core.Database.AudioStationDatabase
{
    public class MusicBrainzAcoustIDResult : AudioStationViewEntityBase
    {
        public int TagSmallId { get; set; }
        public Guid AcoustIDLookupId { get; set; }
        public Guid MusicBrainzRecordingId { get; set; }
        public double Score { get; set; }
        public string FileName { get; set; }
    }
}
