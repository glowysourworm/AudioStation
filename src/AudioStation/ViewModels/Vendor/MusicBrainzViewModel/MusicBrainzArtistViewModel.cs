using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    public class MusicBrainzArtistViewModel : ViewModelBase, IArtist
    {
        IArea? _area;

        public IArea? Area
        {
            get { return _area; }
            set { this.RaiseAndSetIfChanged(ref _area, value); }
        }
        IArea? _beginArea;

        public IArea? BeginArea
        {
            get { return _beginArea; }
            set { this.RaiseAndSetIfChanged(ref _beginArea, value); }
        }
        string? _country;

        public string? Country
        {
            get { return _country; }
            set { this.RaiseAndSetIfChanged(ref _country, value); }
        }
        IArea? _endArea;

        public IArea? EndArea
        {
            get { return _endArea; }
            set { this.RaiseAndSetIfChanged(ref _endArea, value); }
        }
        string? _gender;

        public string? Gender
        {
            get { return _gender; }
            set { this.RaiseAndSetIfChanged(ref _gender, value); }
        }
        Guid? _genderId;

        public Guid? GenderId
        {
            get { return _genderId; }
            set { this.RaiseAndSetIfChanged(ref _genderId, value); }
        }
        IReadOnlyList<string>? _ipis;

        public IReadOnlyList<string>? Ipis
        {
            get { return _ipis; }
            set { this.RaiseAndSetIfChanged(ref _ipis, value); }
        }
        IReadOnlyList<string>? _isnis;

        public IReadOnlyList<string>? Isnis
        {
            get { return _isnis; }
            set { this.RaiseAndSetIfChanged(ref _isnis, value); }
        }
        ILifeSpan? _lifeSpan;

        public ILifeSpan? LifeSpan
        {
            get { return _lifeSpan; }
            set { this.RaiseAndSetIfChanged(ref _lifeSpan, value); }
        }
        IReadOnlyList<IRecording>? _recordings;

        public IReadOnlyList<IRecording>? Recordings
        {
            get { return _recordings; }
            set { this.RaiseAndSetIfChanged(ref _recordings, value); }
        }
        IReadOnlyList<IReleaseGroup>? _releaseGroups;

        public IReadOnlyList<IReleaseGroup>? ReleaseGroups
        {
            get { return _releaseGroups; }
            set { this.RaiseAndSetIfChanged(ref _releaseGroups, value); }
        }
        IReadOnlyList<IRelease>? _releases;

        public IReadOnlyList<IRelease>? Releases
        {
            get { return _releases; }
            set { this.RaiseAndSetIfChanged(ref _releases, value); }
        }
        string? _sortName;

        public string? SortName
        {
            get { return _sortName; }
            set { this.RaiseAndSetIfChanged(ref _sortName, value); }
        }
        IReadOnlyList<IWork>? _works;

        public IReadOnlyList<IWork>? Works
        {
            get { return _works; }
            set { this.RaiseAndSetIfChanged(ref _works, value); }
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
        string? _disambiguation;

        public string? Disambiguation
        {
            get { return _disambiguation; }
            set { this.RaiseAndSetIfChanged(ref _disambiguation, value); }
        }
        string? _name;

        public string? Name
        {
            get { return _name; }
            set { this.RaiseAndSetIfChanged(ref _name, value); }
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
        string? _type;

        public string? Type
        {
            get { return _type; }
            set { this.RaiseAndSetIfChanged(ref _type, value); }
        }
        Guid? _typeId;

        public Guid? TypeId
        {
            get { return _typeId; }
            set { this.RaiseAndSetIfChanged(ref _typeId, value); }
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

        public MusicBrainzArtistViewModel()
        {

        }
    }
}
