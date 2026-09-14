namespace AudioStation.Core.Database.AudioStationDatabase.Interface
{
    public interface IAcoustIDLookupResult
    {
        int Id { get; set; }
        string FileName { get; set; }
        string Message { get; set; }
        Guid? LookupId { get; set; }
        Guid? MusicBrainzRecordingId { get; set; }
        double? Score { get; set; }
        public int? ImportWorkflowId { get; set; }
    }
}
