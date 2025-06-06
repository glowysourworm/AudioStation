namespace AudioStation.Core.Model
{
    public class Playlist
    {
        public string Name { get; set; }
        public List<LibraryEntry> Tracks { get; set; }

        public Playlist()
        {
            this.Name = string.Empty;
            this.Tracks = new List<LibraryEntry>();
        }
    }
}
