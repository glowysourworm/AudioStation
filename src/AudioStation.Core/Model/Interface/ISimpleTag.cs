namespace AudioStation.Core.Model.Interface
{
    /// <summary>
    /// This represents the bare minimum tag to import / use the mp3 file.
    /// </summary>
    public interface ISimpleTag
    {
        public string Album { get; set; }
        public string FirstAlbumArtist { get; set; }
        public string Title { get; set; }
        public string FirstGenre { get; set; }
        public uint Track { get; set; }
        public uint TrackCount { get; set; }
        public uint Disc { get; set; }
        public uint DiscCount { get; set; }
    }
}
