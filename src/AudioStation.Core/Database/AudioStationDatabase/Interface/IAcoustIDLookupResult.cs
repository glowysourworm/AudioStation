namespace AudioStation.Core.Database.AudioStationDatabase.Interface
{
    public interface IAcoustIDLookupResult
    {
        int Id { get; set; }
        string FileName { get; set; }
        Guid LookupId { get; set; }
        Guid MusicBrainzRecordingId { get; set; }
        double Score { get; set; }
        string Fingerprint { get; set; }
        public int? ImportWorkflowId { get; set; }
    }
}
