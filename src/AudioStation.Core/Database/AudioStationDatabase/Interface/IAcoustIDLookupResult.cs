namespace AudioStation.Core.Database.AudioStationDatabase.Interface
{
    public interface IAcoustIDLookupResult
    {
        Guid Id { get; set; }
        string FileName { get; set; }
        Guid LookupId { get; set; }
        Guid MusicBrainzRecordingId { get; set; }
        double Score { get; set; }
    }
}
