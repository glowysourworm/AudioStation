using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    public class MusicBrainzRecordingViewModel : ViewModelBase, IRecording
    {

        IReadOnlyList<INameCredit>? _artistCredit;

        public IReadOnlyList<INameCredit>? ArtistCredit
        {
            get { return _artistCredit; }
            set { this.RaiseAndSetIfChanged(ref _artistCredit, value); }
        }
        PartialDate? _firstReleaseDate;

        public PartialDate? FirstReleaseDate
        {
            get { return _firstReleaseDate; }
            set { this.RaiseAndSetIfChanged(ref _firstReleaseDate, value); }
        }
        IReadOnlyList<string>? _isrcs;

        public IReadOnlyList<string>? Isrcs
        {
            get { return _isrcs; }
            set { this.RaiseAndSetIfChanged(ref _isrcs, value); }
        }
        TimeSpan? _length;

        public TimeSpan? Length
        {
            get { return _length; }
            set { this.RaiseAndSetIfChanged(ref _length, value); }
        }
        IReadOnlyList<IRelease>? _releases;

        public IReadOnlyList<IRelease>? Releases
        {
            get { return _releases; }
            set { this.RaiseAndSetIfChanged(ref _releases, value); }
        }
        bool _video;

        public bool Video
        {
            get { return _video; }
            set { this.RaiseAndSetIfChanged(ref _video, value); }
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
        IRating? _rating;

        public IRating? Rating
        {
            get { return _rating; }
            set { this.RaiseAndSetIfChanged(ref _rating, value); }
        }
        IRating? _userRating;

        public IRating? UserRating
        {
            get { return _userRating; }
            set { this.RaiseAndSetIfChanged(ref _userRating, value); }
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

        public MusicBrainzRecordingViewModel()
        {

        }
    }
}
