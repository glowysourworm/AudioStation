namespace AudioStation.Core.Model
{
    public class Library
    {
        public List<LibraryEntry> Titles { get; set; }
        public List<Artist> Artists { get; set; }
        public List<Album> Albums { get; set; }
        public List<string> Genres { get; set; }
        public List<Playlist> Playlists { get; set; }

        public Library()
        {
            this.Titles = new List<LibraryEntry>();
            this.Artists = new List<Artist>();
            this.Albums = new List<Album>();
            this.Genres = new List<string>();
            this.Playlists = new List<Playlist>();
        }
    }
}
