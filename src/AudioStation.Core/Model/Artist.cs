namespace AudioStation.Core.Model
{
    public class Artist
    {
        public string Name { get; set; }

        // Aggregated data
        public List<Album> Albums {get;set;}

        public Artist()
        {
            this.Name = string.Empty;
            this.Albums = new List<Album>();
        }
    }
}
