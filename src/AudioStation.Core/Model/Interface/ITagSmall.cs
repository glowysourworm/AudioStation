namespace AudioStation.Core.Model.Interface
{
    public interface ITagSmall
    {
        string? AlbumArtist { get; set; }
        string? Album { get; set; }
        string? Title { get; set; }
        string? Genre { get; set; }
        int? TrackNumber { get; set; }
        int? TrackTotal { get; set; }
        int? MediaNumber { get; set; }
        int? MediaTotal { get; set; }
        string? MediaFormat { get; set; }
        int? DurationMilliseconds { get; set; }
        int? Year { get; set; }
    }
}
