using ATL;

using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Utility;
using AudioStation.Core.Utility.RecursiveComparer.Attribute;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.ATLViewModel
{
    /// <summary>
    /// IAudioStationTag based view model. This is based on the ATL library.
    /// </summary>
    public class TagViewModel : ViewModelBase, IAudioStationTag
    {
        #region (private) Fields
        IDictionary<string, string> _additionalFields;
        string _album;
        string _albumArtist;
        IList<string> _albumArtists;
        string _artist;
        string _audioFormat;
        string _audioSourceUrl;
        int _bitDepth;
        int _bitRate;
        float? _bPM;
        string _catalogNumber;
        int _channels;
        IList<ChapterInfo> _chapters;
        string _chaptersTableDescription;
        string _comment;
        string _composer;
        string _conductor;
        string _copyright;
        DateTime _date;
        ushort _discNumber;
        ushort _discTotal;
        TimeSpan _duration;
        IList<PictureInfo> _embeddedPictures;
        string _encodedBy;
        string _encoder;
        string _generalDescription;
        string _genre;
        IList<string> _genres;
        string _group;
        string _involvedPeople;
        bool _isDateYearOnly;
        bool _isOriginalReleaseDateYearOnly;
        bool _isVariableBitRate;
        string _iSRC;
        string _language;
        string _longDescription;
        string _lyricist;
        IList<LyricsInfo> _lyrics;
        IList<Format> _metadataFormats;
        string _originalArtist;
        string _originalAlbum;
        DateTime _originalReleaseDate;
        float? _popularity;
        string _productId;
        string _publisher;
        DateTime _publishingDate;
        double _sampleRate;
        string _seriesPart;
        string _seriesTitle;
        string _sortAlbum;
        string _sortAlbumArtist;
        string _sortArtist;
        string _sortTitle;
        string _title;
        uint _track;                     // Parsed from track number string
        string _trackNumber;
        ushort _trackTotal;
        int _year;
        #endregion

        public IDictionary<string, string> AdditionalFields
        {
            get { return _additionalFields; }
            set { this.RaiseAndSetIfChanged(ref _additionalFields, value); }
        }
        public string Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
        public string AlbumArtist
        {
            get { return _albumArtist; }
            set { this.RaiseAndSetIfChanged(ref _albumArtist, value); }
        }
        public IList<string> AlbumArtists
        {
            get { return _albumArtists; }
            set { this.RaiseAndSetIfChanged(ref _albumArtists, value); }
        }
        public string Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        public string AudioFormat
        {
            get { return _audioFormat; }
            set { this.RaiseAndSetIfChanged(ref _audioFormat, value); }
        }
        public string AudioSourceUrl
        {
            get { return _audioSourceUrl; }
            set { this.RaiseAndSetIfChanged(ref _audioSourceUrl, value); }
        }
        public int BitDepth
        {
            get { return _bitDepth; }
            set { this.RaiseAndSetIfChanged(ref _bitDepth, value); }
        }
        public int BitRate
        {
            get { return _bitRate; }
            set { this.RaiseAndSetIfChanged(ref _bitRate, value); }
        }
        public float? BPM
        {
            get { return _bPM; }
            set { this.RaiseAndSetIfChanged(ref _bPM, value); }
        }
        public string CatalogNumber
        {
            get { return _catalogNumber; }
            set { this.RaiseAndSetIfChanged(ref _catalogNumber, value); }
        }
        public int Channels
        {
            get { return _channels; }
            set { this.RaiseAndSetIfChanged(ref _channels, value); }
        }
        public IList<ChapterInfo> Chapters
        {
            get { return _chapters; }
            set { this.RaiseAndSetIfChanged(ref _chapters, value); }
        }
        public string ChaptersTableDescription
        {
            get { return _chaptersTableDescription; }
            set { this.RaiseAndSetIfChanged(ref _chaptersTableDescription, value); }
        }
        public string Comment
        {
            get { return _comment; }
            set { this.RaiseAndSetIfChanged(ref _comment, value); }
        }
        public string Composer
        {
            get { return _composer; }
            set { this.RaiseAndSetIfChanged(ref _composer, value); }
        }
        public string Conductor
        {
            get { return _conductor; }
            set { this.RaiseAndSetIfChanged(ref _conductor, value); }
        }
        public string Copyright
        {
            get { return _copyright; }
            set { this.RaiseAndSetIfChanged(ref _copyright, value); }
        }
        public DateTime Date
        {
            get { return _date; }
            set { this.RaiseAndSetIfChanged(ref _date, value); }
        }
        public ushort DiscNumber
        {
            get { return _discNumber; }
            set { this.RaiseAndSetIfChanged(ref _discNumber, value); }
        }
        public ushort DiscTotal
        {
            get { return _discTotal; }
            set { this.RaiseAndSetIfChanged(ref _discTotal, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }
        public IList<PictureInfo> EmbeddedPictures
        {
            get { return _embeddedPictures; }
            set { this.RaiseAndSetIfChanged(ref _embeddedPictures, value); }
        }
        public string EncodedBy
        {
            get { return _encodedBy; }
            set { this.RaiseAndSetIfChanged(ref _encodedBy, value); }
        }
        public string Encoder
        {
            get { return _encoder; }
            set { this.RaiseAndSetIfChanged(ref _encoder, value); }
        }
        public string GeneralDescription
        {
            get { return _generalDescription; }
            set { this.RaiseAndSetIfChanged(ref _generalDescription, value); }
        }
        public string Genre
        {
            get { return _genre; }
            set { this.RaiseAndSetIfChanged(ref _genre, value); }
        }
        public IList<string> Genres
        {
            get { return _genres; }
            set { this.RaiseAndSetIfChanged(ref _genres, value); }
        }
        public string Group
        {
            get { return _group; }
            set { this.RaiseAndSetIfChanged(ref _group, value); }
        }
        public string InvolvedPeople
        {
            get { return _involvedPeople; }
            set { this.RaiseAndSetIfChanged(ref _involvedPeople, value); }
        }
        public bool IsDateYearOnly
        {
            get { return _isDateYearOnly; }
            set { this.RaiseAndSetIfChanged(ref _isDateYearOnly, value); }
        }
        public bool IsOriginalReleaseDateYearOnly
        {
            get { return _isOriginalReleaseDateYearOnly; }
            set { this.RaiseAndSetIfChanged(ref _isOriginalReleaseDateYearOnly, value); }
        }
        public bool IsVariableBitRate
        {
            get { return _isVariableBitRate; }
            set { this.RaiseAndSetIfChanged(ref _isVariableBitRate, value); }
        }
        public string ISRC
        {
            get { return _iSRC; }
            set { this.RaiseAndSetIfChanged(ref _iSRC, value); }
        }
        public string Language
        {
            get { return _language; }
            set { this.RaiseAndSetIfChanged(ref _language, value); }
        }
        public string LongDescription
        {
            get { return _longDescription; }
            set { this.RaiseAndSetIfChanged(ref _longDescription, value); }
        }
        public string Lyricist
        {
            get { return _lyricist; }
            set { this.RaiseAndSetIfChanged(ref _lyricist, value); }
        }
        public IList<LyricsInfo> Lyrics
        {
            get { return _lyrics; }
            set { this.RaiseAndSetIfChanged(ref _lyrics, value); }
        }

        [RecursiveCompareIgnore]
        public IList<Format> MetadataFormats
        {
            get { return _metadataFormats; }
            set { this.RaiseAndSetIfChanged(ref _metadataFormats, value); }
        }
        public string OriginalArtist
        {
            get { return _originalArtist; }
            set { this.RaiseAndSetIfChanged(ref _originalArtist, value); }
        }
        public string OriginalAlbum
        {
            get { return _originalAlbum; }
            set { this.RaiseAndSetIfChanged(ref _originalAlbum, value); }
        }
        public DateTime OriginalReleaseDate
        {
            get { return _originalReleaseDate; }
            set { this.RaiseAndSetIfChanged(ref _originalReleaseDate, value); }
        }
        public float? Popularity
        {
            get { return _popularity; }
            set { this.RaiseAndSetIfChanged(ref _popularity, value); }
        }
        public string ProductId
        {
            get { return _productId; }
            set { this.RaiseAndSetIfChanged(ref _productId, value); }
        }
        public string Publisher
        {
            get { return _publisher; }
            set { this.RaiseAndSetIfChanged(ref _publisher, value); }
        }
        public DateTime PublishingDate
        {
            get { return _publishingDate; }
            set { this.RaiseAndSetIfChanged(ref _publishingDate, value); }
        }
        public double SampleRate
        {
            get { return _sampleRate; }
            set { this.RaiseAndSetIfChanged(ref _sampleRate, value); }
        }
        public string SeriesPart
        {
            get { return _seriesPart; }
            set { this.RaiseAndSetIfChanged(ref _seriesPart, value); }
        }
        public string SeriesTitle
        {
            get { return _seriesTitle; }
            set { this.RaiseAndSetIfChanged(ref _seriesTitle, value); }
        }
        public string SortAlbum
        {
            get { return _sortAlbum; }
            set { this.RaiseAndSetIfChanged(ref _sortAlbum, value); }
        }
        public string SortAlbumArtist
        {
            get { return _sortAlbumArtist; }
            set { this.RaiseAndSetIfChanged(ref _sortAlbumArtist, value); }
        }
        public string SortArtist
        {
            get { return _sortArtist; }
            set { this.RaiseAndSetIfChanged(ref _sortArtist, value); }
        }
        public string SortTitle
        {
            get { return _sortTitle; }
            set { this.RaiseAndSetIfChanged(ref _sortTitle, value); }
        }
        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        public uint Track
        {
            get { return _track; }
            set { this.RaiseAndSetIfChanged(ref _track, value); }
        }
        public string TrackNumber
        {
            get { return _trackNumber; }
            set { this.RaiseAndSetIfChanged(ref _trackNumber, value); }
        }
        public ushort TrackTotal
        {
            get { return _trackTotal; }
            set { this.RaiseAndSetIfChanged(ref _trackTotal, value); }
        }
        public int Year
        {
            get { return _year; }
            set { this.RaiseAndSetIfChanged(ref _year, value); }
        }

        public TagViewModel()
        {
            this.AlbumArtists = new List<string>();
            this.Chapters = new List<ChapterInfo>();
            this.EmbeddedPictures = new List<PictureInfo>();
            this.Genres = new List<string>();
            this.Lyrics = new List<LyricsInfo>();
            this.MetadataFormats = new List<Format>();
        }
        public TagViewModel(IAudioStationTag tag)
            : this()
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
            uint.TryParse(this.TrackNumber, out track);
            this.Track = track;

            // Year
            this.Year = this.Date.Year;
        }
    }
}
