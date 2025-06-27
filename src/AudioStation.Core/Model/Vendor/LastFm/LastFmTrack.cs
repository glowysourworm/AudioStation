using SimpleWpf.Extensions;

namespace AudioStation.Core.Model.Vendor
{
    public class LastFmTrack
    {
        public string ArtistImage { get; set; }
        public string ArtistUrl { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }

        public LastFmTrack()
        {
            this.ArtistUrl = string.Empty;
            this.ArtistImage = string.Empty;
            this.Image = string.Empty;
            this.Url = string.Empty;
        }
    }
}
