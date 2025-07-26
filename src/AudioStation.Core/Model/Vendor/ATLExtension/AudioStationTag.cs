using ATL;

using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Utility;
using AudioStation.Core.Utility.RecursiveComparer.Attribute;

using AutoMapper.Configuration.Annotations;

namespace AudioStation.Core.Model.Vendor.ATLExtension
{
    public class AudioStationTag : IAudioStationTag
    {
        public IDictionary<string, string> AdditionalFields { get; set; }
        public string AudioFormat { get; set; }
        public string Album { get; set; }
        public string AlbumArtist { get; set; }
        public IList<string> AlbumArtists { get; set; }
        public string Artist { get; set; }
        public string AudioSourceUrl { get; set; }
        public int BitDepth { get; set; }
        public int BitRate { get; set; }
        public float? BPM { get; set; }
        public string CatalogNumber { get; set; }
        public int Channels { get; set; }
        public IList<ChapterInfo> Chapters { get; set; }
        public string ChaptersTableDescription { get; set; }
        public string Comment { get; set; }
        public string Composer { get; set; }
        public string Conductor { get; set; }
        public string Copyright { get; set; }
        public DateTime Date { get; set; }
        public ushort DiscNumber { get; set; }
        public ushort DiscTotal { get; set; }
        public TimeSpan Duration { get; set; }
        public IList<PictureInfo> EmbeddedPictures { get; set; }
        public string EncodedBy { get; set; }
        public string Encoder { get; set; }
        public string GeneralDescription { get; set; }
        public string Genre { get; set; }
        public IList<string> Genres { get; set; }
        public string Group { get; set; }
        public string InvolvedPeople { get; set; }
        public bool IsDateYearOnly { get; set; }
        public bool IsVariableBitRate { get; set; }
        public bool IsOriginalReleaseDateYearOnly { get; set; }
        public string ISRC { get; set; }
        public string Language { get; set; }
        public string LongDescription { get; set; }
        public string Lyricist { get; set; }
        public IList<LyricsInfo> Lyrics { get; set; }

        [RecursiveCompareIgnore]
        public IList<Format> MetadataFormats { get; set; }
        public string OriginalArtist { get; set; }
        public string OriginalAlbum { get; set; }
        public DateTime OriginalReleaseDate { get; set; }
        public float? Popularity { get; set; }
        public string ProductId { get; set; }
        public string Publisher { get; set; }
        public DateTime PublishingDate { get; set; }
        public double SampleRate { get; set; }
        public string SeriesPart { get; set; }
        public string SeriesTitle { get; set; }
        public string SortAlbum { get; set; }
        public string SortAlbumArtist { get; set; }
        public string SortArtist { get; set; }
        public string SortTitle { get; set; }
        public string Title { get; set; }
        public uint Track { get; set; }
        public string TrackNumber { get; set; }
        public ushort TrackTotal { get; set; }
        public int Year { get; set; }

        public AudioStationTag()
        {
            this.AlbumArtists = new List<string>();
            this.Genres = new List<string>();
        }
        public AudioStationTag(IAudioStationTag tag)
        {
            ApplicationHelpers.MapOnto(tag, this);
        }

        public void ToATL()
        {
            // AlbumArtist
            this.AlbumArtist = this.AlbumArtists.Any() ? this.AlbumArtists[0] : string.Empty;

            // Genre
            this.Genre = this.Genres.Any() ? this.Genres[0] : string.Empty;

            // TrackNumber (string)
            this.TrackNumber = this.Track.ToString();

            // Date
            if (this.Year >= DateTime.MinValue.Year)
                this.Date = new DateTime(this.Year, 1, 1);

            else
                this.Date = DateTime.MinValue;
        }

        public void FromATL()
        {
            // AlbumArtist Collection
            if (!string.IsNullOrWhiteSpace(this.AlbumArtist) &&
                !this.AlbumArtists.Contains(this.AlbumArtist))
                this.AlbumArtists.Add(this.AlbumArtist);

            // Genre Collection
            if (!string.IsNullOrWhiteSpace(this.Genre) &&
                !this.Genres.Contains(this.Genre))
                this.Genres.Add(this.Genre);

            // Track
            uint track = 0;
            if (uint.TryParse(this.TrackNumber, out track))
                this.Track = track;

            // Year
            this.Year = this.Date.Year;
        }
    }
}
