namespace AudioStation.Core.Model.Vendor
{
    public class AudioDBArtist
    {
        public List<AudioDBAlbum> Albums { get; set; }
        public int IdArtist { get; set; }
        public int IdLabel { get; set; }
        public string MusicBrainzId { get; set; }
        public int YearFormed { get; set; }
        public string ArtistBio { get; set; }
        public string ArtistName { get; set; }
        public string ArtistThumb { get; set; }
        public string ArtistLogo { get; set; }
        public string ArtistCutout { get; set; }
        public string ArtistClearart { get; set; }
        public string ArtistWideThumb { get; set; }
        public string ArtistFanart { get; set; }
        public string ArtistFanart2 { get; set; }
        public string ArtistFanart3 { get; set; }
        public string ArtistFanart4 { get; set; }
        public string ArtistBanner { get; set; }
        public string Website { get; set; }
        public string Facebook { get; set; }
        public string Country { get; set; }

    }
}
