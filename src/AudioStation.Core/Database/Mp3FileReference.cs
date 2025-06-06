namespace AudioStation.Core.Database
{
    public class Mp3FileReference
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Title { get; set; }
        public string Album { get; set; }
        public string PrimaryArtist { get; set; }
        public int Track { get; set; }

        public Mp3FileReference(int Id_, string FileName_, string Title_, string Album_, string PrimaryArtist_, int Track_)
        {
            this.Id = Id_;
            this.FileName = FileName_;
            this.Title = Title_;
            this.Album = Album_;
            this.PrimaryArtist = PrimaryArtist_;
            this.Track = Track_;
        }
    }
}
