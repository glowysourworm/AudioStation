namespace AudioStation.Core.Model
{
    public class Album
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public uint Year { get; set; }

        // Aggregated data
        public List<LibraryEntry> Tracks { get; set; }

        public Album()
        {
            this.Name = string.Empty;
            this.Tracks = new List<LibraryEntry>();
        }
    }
}
