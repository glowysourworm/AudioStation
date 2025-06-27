namespace AudioStation.Core.Model.Vendor
{
    public class LastFmNowPlaying
    {
        public int ArtistYearFormed { get; set; }
        public string BioSummary { get; set; }
        public string BioContent { get; set; }
        public string ArtistUrl { get; set; }
        public string ArtistMainImage { get; set; }
        public string AlbumImage { get; set; }
        public string AlbumUrl { get; set; }
        public List<LastFmTrack> Tracks { get; set; }

        public LastFmNowPlaying()
        {
            this.BioSummary = string.Empty;
            this.BioContent = string.Empty;
            this.ArtistUrl = string.Empty;
            this.ArtistMainImage = string.Empty;
            this.AlbumImage = string.Empty;
            this.AlbumUrl = string.Empty;
            this.Tracks = new List<LastFmTrack>();
        }
    }
}
