namespace AudioStation.Core.Model.Vendor
{
    public class SpotifyNowPlaying
    {
        public string ArtistUrl { get; set; }
        public string AlbumUrl { get; set; }
        public List<string> AlbumExtenralUrls { get; set; }
        public List<string> ArtistExternalUrls { get; set; }
        public List<string> ArtistImages { get; set; }
        public List<string> AlbumImages { get; set; }
        public List<string> CombinedImages { get; set; }

        public SpotifyNowPlaying()
        {
            this.ArtistUrl = string.Empty;
            this.AlbumUrl = string.Empty;
            this.AlbumImages = new List<string>();
            this.ArtistImages = new List<string>();
            this.CombinedImages = new List<string>();
            this.AlbumExtenralUrls = new List<string>();
            this.ArtistExternalUrls = new List<string>();
        }
    }
}
