namespace AudioStation.Core.Model.Interface
{
    public interface ITagSmall
    {
        string? AlbumArtist { get; set; }
        string? Album { get; set; }
        string? Title { get; set; }
        string? Genre { get; set; }
        int TrackNumber { get; set; }
        int TrackTotal { get; set; }
        int DiscNumber { get; set; }
        int DiscTotal { get; set; }
    }
}
