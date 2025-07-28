
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    public class MusicBrainzReleaseViewModel : ViewModelBase, IRelease
    {
        IReadOnlyList<INameCredit>? _artistCredit;

        public IReadOnlyList<INameCredit>? ArtistCredit
        {
            get { return _artistCredit; }
            set { this.RaiseAndSetIfChanged(ref _artistCredit, value); }
        }
        string? _asin;

        public string? Asin
        {
            get { return _asin; }
            set { this.RaiseAndSetIfChanged(ref _asin, value); }
        }
        string? _barcode;

        public string? Barcode
        {
            get { return _barcode; }
            set { this.RaiseAndSetIfChanged(ref _barcode, value); }
        }
        IReadOnlyList<ICollection>? _collections;

        public IReadOnlyList<ICollection>? Collections
        {
            get { return _collections; }
            set { this.RaiseAndSetIfChanged(ref _collections, value); }
        }
        string? _country;

        public string? Country
        {
            get { return _country; }
            set { this.RaiseAndSetIfChanged(ref _country, value); }
        }
        ICoverArtArchive? _coverArtArchive;

        public ICoverArtArchive? CoverArtArchive
        {
            get { return _coverArtArchive; }
            set { this.RaiseAndSetIfChanged(ref _coverArtArchive, value); }
        }
        PartialDate? _date;

        public PartialDate? Date
        {
            get { return _date; }
            set { this.RaiseAndSetIfChanged(ref _date, value); }
        }
        IReadOnlyList<ILabelInfo>? _labelInfo;

        public IReadOnlyList<ILabelInfo>? LabelInfo
        {
            get { return _labelInfo; }
            set { this.RaiseAndSetIfChanged(ref _labelInfo, value); }
        }
        IReadOnlyList<IMedium>? _media;

        public IReadOnlyList<IMedium>? Media
        {
            get { return _media; }
            set { this.RaiseAndSetIfChanged(ref _media, value); }
        }
        string? _packaging;

        public string? Packaging
        {
            get { return _packaging; }
            set { this.RaiseAndSetIfChanged(ref _packaging, value); }
        }
        Guid? _packagingId;

        public Guid? PackagingId
        {
            get { return _packagingId; }
            set { this.RaiseAndSetIfChanged(ref _packagingId, value); }
        }
        string? _quality;

        public string? Quality
        {
            get { return _quality; }
            set { this.RaiseAndSetIfChanged(ref _quality, value); }
        }
        IReadOnlyList<IReleaseEvent>? _releaseEvents;

        public IReadOnlyList<IReleaseEvent>? ReleaseEvents
        {
            get { return _releaseEvents; }
            set { this.RaiseAndSetIfChanged(ref _releaseEvents, value); }
        }
        IReleaseGroup? _releaseGroup;

        public IReleaseGroup? ReleaseGroup
        {
            get { return _releaseGroup; }
            set { this.RaiseAndSetIfChanged(ref _releaseGroup, value); }
        }
        string? _status;

        public string? Status
        {
            get { return _status; }
            set { this.RaiseAndSetIfChanged(ref _status, value); }
        }
        Guid? _statusId;

        public Guid? StatusId
        {
            get { return _statusId; }
            set { this.RaiseAndSetIfChanged(ref _statusId, value); }
        }
        ITextRepresentation? _textRepresentation;

        public ITextRepresentation? TextRepresentation
        {
            get { return _textRepresentation; }
            set { this.RaiseAndSetIfChanged(ref _textRepresentation, value); }
        }
        IReadOnlyList<IAlias>? _aliases;

        public IReadOnlyList<IAlias>? Aliases
        {
            get { return _aliases; }
            set { this.RaiseAndSetIfChanged(ref _aliases, value); }
        }
        string? _annotation;

        public string? Annotation
        {
            get { return _annotation; }
            set { this.RaiseAndSetIfChanged(ref _annotation, value); }
        }
        IReadOnlyList<IRelationship>? _relationships;

        public IReadOnlyList<IRelationship>? Relationships
        {
            get { return _relationships; }
            set { this.RaiseAndSetIfChanged(ref _relationships, value); }
        }
        IReadOnlyList<IGenre>? _genres;

        public IReadOnlyList<IGenre>? Genres
        {
            get { return _genres; }
            set { this.RaiseAndSetIfChanged(ref _genres, value); }
        }
        IReadOnlyList<ITag>? _tags;

        public IReadOnlyList<ITag>? Tags
        {
            get { return _tags; }
            set { this.RaiseAndSetIfChanged(ref _tags, value); }
        }
        IReadOnlyList<IGenre>? _userGenres;

        public IReadOnlyList<IGenre>? UserGenres
        {
            get { return _userGenres; }
            set { this.RaiseAndSetIfChanged(ref _userGenres, value); }
        }
        IReadOnlyList<ITag>? _userTags;

        public IReadOnlyList<ITag>? UserTags
        {
            get { return _userTags; }
            set { this.RaiseAndSetIfChanged(ref _userTags, value); }
        }
        string? _disambiguation;

        public string? Disambiguation
        {
            get { return _disambiguation; }
            set { this.RaiseAndSetIfChanged(ref _disambiguation, value); }
        }
        string? _title;

        public string? Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        EntityType _entityType;

        public EntityType EntityType
        {
            get { return _entityType; }
            set { this.RaiseAndSetIfChanged(ref _entityType, value); }
        }
        Guid _id;

        public Guid Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        IReadOnlyDictionary<string, object?>? _unhandledProperties;

        public IReadOnlyDictionary<string, object?>? UnhandledProperties
        {
            get { return _unhandledProperties; }
            set { this.RaiseAndSetIfChanged(ref _unhandledProperties, value); }
        }

        public MusicBrainzReleaseViewModel()
        {
        }
    }
}
