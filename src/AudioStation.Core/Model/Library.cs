namespace AudioStation.Core.Model
{
    public class Library
    {
        public List<LibraryEntry> Entries { get; set; }
        public List<Artist> Artists { get; set; }
        public List<Album> Albums { get; set; }
        public List<Genre> Genres { get; set; }
        public List<Playlist> Playlists { get; set; }

        public Library()
        {
            this.Entries = new List<LibraryEntry>();
            this.Artists = new List<Artist>();
            this.Albums = new List<Album>();
            this.Genres = new List<Genre>();
            this.Playlists = new List<Playlist>();
        }
    }
}
