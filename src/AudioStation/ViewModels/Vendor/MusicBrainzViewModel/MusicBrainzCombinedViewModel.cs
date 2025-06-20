using System.IO;

using MetaBrainz.MusicBrainz.CoverArt;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    public class MusicBrainzCombinedViewModel : ViewModelBase
    {
        Guid _releaseId;
        Guid _artistId;
        Guid _trackId;
        string _artistCreditName;
        string _asin;
        string _barcode;
        string _releaseCountry;
        ImageViewModel _frontCover;
        ImageViewModel _backCover;
        string _labelCatalogNumber;
        int _labelCode;
        string _labelName;
        string _labelCountry;
        IEnumerable<string> _labelIpis;
        int _mediumTrackCount;
        int _mediumTrackOffset;
        int _mediumDiscCount;
        int _mediumDiscPosition;
        string _mediumFormat;
        string _mediumTitle;
        ITrack _track;
        string _packaging;
        string _quality;
        DateTime _releaseDate;
        string _releaseStatus;
        string _releaseTitle;
        string _annotation;
        IEnumerable<string> _associatedUrls;
        IEnumerable<string> _genres;
        IEnumerable<string> _userGenres;
        IEnumerable<string> _tags;
        IEnumerable<string> _userTags;
        string _disambiguation;
        string _title;

        public Guid ReleaseId
        {
            get { return _releaseId; }
            set { this.RaiseAndSetIfChanged(ref _releaseId, value); }
        }
        public Guid ArtistId
        {
            get { return _artistId; }
            set { this.RaiseAndSetIfChanged(ref _artistId, value); }
        }
        public Guid TrackId
        {
            get { return _trackId; }
            set { this.RaiseAndSetIfChanged(ref _trackId, value); }
        }
        public string ArtistCreditName
        {
            get { return _artistCreditName; }
            set { this.RaiseAndSetIfChanged(ref _artistCreditName, value); }
        }
        public string Asin
        {
            get { return _asin; }
            set { this.RaiseAndSetIfChanged(ref _asin, value); }
        }
        public string Barcode
        {
            get { return _barcode; }
            set { this.RaiseAndSetIfChanged(ref _barcode, value); }
        }
        public string ReleaseCountry
        {
            get { return _releaseCountry; }
            set { this.RaiseAndSetIfChanged(ref _releaseCountry, value); }
        }
        public ImageViewModel FrontCover
        {
            get { return _frontCover; }
            set { this.RaiseAndSetIfChanged(ref _frontCover, value); }
        }
        public ImageViewModel BackCover
        {
            get { return _backCover; }
            set { this.RaiseAndSetIfChanged(ref _backCover, value); }
        }
        public string LabelCatalogNumber
        {
            get { return _labelCatalogNumber; }
            set { this.RaiseAndSetIfChanged(ref _labelCatalogNumber, value); }
        }
        public int LabelCode
        {
            get { return _labelCode; }
            set { this.RaiseAndSetIfChanged(ref _labelCode, value); }
        }
        public string LabelName
        {
            get { return _labelName; }
            set { this.RaiseAndSetIfChanged(ref _labelName, value); }
        }
        public string LabelCountry
        {
            get { return _labelCountry; }
            set { this.RaiseAndSetIfChanged(ref _labelCountry, value); }
        }
        public IEnumerable<string> LabelIpis
        {
            get { return _labelIpis; }
            set { this.RaiseAndSetIfChanged(ref _labelIpis, value); }
        }
        public int MediumTrackCount
        {
            get { return _mediumTrackCount; }
            set { this.RaiseAndSetIfChanged(ref _mediumTrackCount, value); }
        }
        public int MediumTrackOffset
        {
            get { return _mediumTrackOffset; }
            set { this.RaiseAndSetIfChanged(ref _mediumTrackOffset, value); }
        }
        public int MediumDiscCount
        {
            get { return _mediumDiscCount; }
            set { this.RaiseAndSetIfChanged(ref _mediumDiscCount, value); }
        }
        public int MediumDiscPosition
        {
            get { return _mediumDiscPosition; }
            set { this.RaiseAndSetIfChanged(ref _mediumDiscPosition, value); }
        }
        public string MediumFormat
        {
            get { return _mediumFormat; }
            set { this.RaiseAndSetIfChanged(ref _mediumFormat, value); }
        }
        public string MediumTitle
        {
            get { return _mediumTitle; }
            set { this.RaiseAndSetIfChanged(ref _mediumTitle, value); }
        }
        public ITrack Track
        {
            get { return _track; }
            set { this.RaiseAndSetIfChanged(ref _track, value); }
        }
        public string Packaging
        {
            get { return _packaging; }
            set { this.RaiseAndSetIfChanged(ref _packaging, value); }
        }
        public string Quality
        {
            get { return _quality; }
            set { this.RaiseAndSetIfChanged(ref _quality, value); }
        }
        public DateTime ReleaseDate
        {
            get { return _releaseDate; }
            set { this.RaiseAndSetIfChanged(ref _releaseDate, value); }
        }
        public string ReleaseStatus
        {
            get { return _releaseStatus; }
            set { this.RaiseAndSetIfChanged(ref _releaseStatus, value); }
        }
        public string ReleaseTitle
        {
            get { return _releaseTitle; }
            set { this.RaiseAndSetIfChanged(ref _releaseTitle, value); }
        }
        public string Annotation
        {
            get { return _annotation; }
            set { this.RaiseAndSetIfChanged(ref _annotation, value); }
        }
        public IEnumerable<string> AssociatedUrls
        {
            get { return _associatedUrls; }
            set { this.RaiseAndSetIfChanged(ref _associatedUrls, value); }
        }
        public IEnumerable<string> Genres
        {
            get { return _genres; }
            set { this.RaiseAndSetIfChanged(ref _genres, value); }
        }
        public IEnumerable<string> UserGenres
        {
            get { return _userGenres; }
            set { this.RaiseAndSetIfChanged(ref _userGenres, value); }
        }
        public IEnumerable<string> Tags
        {
            get { return _tags; }
            set { this.RaiseAndSetIfChanged(ref _tags, value); }
        }
        public IEnumerable<string> UserTags
        {
            get { return _userTags; }
            set { this.RaiseAndSetIfChanged(ref _userTags, value); }
        }
        public string Disambiguation
        {
            get { return _disambiguation; }
            set { this.RaiseAndSetIfChanged(ref _disambiguation, value); }
        }
        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }

        public MusicBrainzCombinedViewModel()
        {

        }
    }
}
