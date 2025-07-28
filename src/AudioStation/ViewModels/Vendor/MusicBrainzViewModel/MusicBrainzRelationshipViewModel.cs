
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    internal class MusicBrainzRelationshipViewModel : ViewModelBase, IRelationship
    {
        IArea? _area;

        public IArea? Area
        {
            get { return _area; }
            set { this.RaiseAndSetIfChanged(ref _area, value); }
        }
        IArtist? _artist;

        public IArtist? Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        IReadOnlyList<string>? _attributes;

        public IReadOnlyList<string>? Attributes
        {
            get { return _attributes; }
            set { this.RaiseAndSetIfChanged(ref _attributes, value); }
        }
        IReadOnlyDictionary<string, string>? _attributeCredits;

        public IReadOnlyDictionary<string, string>? AttributeCredits
        {
            get { return _attributeCredits; }
            set { this.RaiseAndSetIfChanged(ref _attributeCredits, value); }
        }
        IReadOnlyDictionary<string, Guid>? _attributeIds;

        public IReadOnlyDictionary<string, Guid>? AttributeIds
        {
            get { return _attributeIds; }
            set { this.RaiseAndSetIfChanged(ref _attributeIds, value); }
        }
        IReadOnlyDictionary<string, string>? _attributeValues;

        public IReadOnlyDictionary<string, string>? AttributeValues
        {
            get { return _attributeValues; }
            set { this.RaiseAndSetIfChanged(ref _attributeValues, value); }
        }
        PartialDate? _begin;

        public PartialDate? Begin
        {
            get { return _begin; }
            set { this.RaiseAndSetIfChanged(ref _begin, value); }
        }
        string? _direction;

        public string? Direction
        {
            get { return _direction; }
            set { this.RaiseAndSetIfChanged(ref _direction, value); }
        }
        PartialDate? _end;

        public PartialDate? End
        {
            get { return _end; }
            set { this.RaiseAndSetIfChanged(ref _end, value); }
        }
        bool _ended;

        public bool Ended
        {
            get { return _ended; }
            set { this.RaiseAndSetIfChanged(ref _ended, value); }
        }
        IEvent? _event;

        public IEvent? Event
        {
            get { return _event; }
            set { this.RaiseAndSetIfChanged(ref _event, value); }
        }
        IInstrument? _instrument;

        public IInstrument? Instrument
        {
            get { return _instrument; }
            set { this.RaiseAndSetIfChanged(ref _instrument, value); }
        }
        ILabel? _label;

        public ILabel? Label
        {
            get { return _label; }
            set { this.RaiseAndSetIfChanged(ref _label, value); }
        }
        int? _orderingKey;

        public int? OrderingKey
        {
            get { return _orderingKey; }
            set { this.RaiseAndSetIfChanged(ref _orderingKey, value); }
        }
        IPlace? _place;

        public IPlace? Place
        {
            get { return _place; }
            set { this.RaiseAndSetIfChanged(ref _place, value); }
        }
        IRecording? _recording;

        public IRecording? Recording
        {
            get { return _recording; }
            set { this.RaiseAndSetIfChanged(ref _recording, value); }
        }
        IRelease? _release;

        public IRelease? Release
        {
            get { return _release; }
            set { this.RaiseAndSetIfChanged(ref _release, value); }
        }
        IReleaseGroup? _releaseGroup;

        public IReleaseGroup? ReleaseGroup
        {
            get { return _releaseGroup; }
            set { this.RaiseAndSetIfChanged(ref _releaseGroup, value); }
        }
        ISeries? _series;

        public ISeries? Series
        {
            get { return _series; }
            set { this.RaiseAndSetIfChanged(ref _series, value); }
        }
        string? _sourceCredit;

        public string? SourceCredit
        {
            get { return _sourceCredit; }
            set { this.RaiseAndSetIfChanged(ref _sourceCredit, value); }
        }
        IRelatableEntity? _target;

        public IRelatableEntity? Target
        {
            get { return _target; }
            set { this.RaiseAndSetIfChanged(ref _target, value); }
        }
        string? _targetCredit;

        public string? TargetCredit
        {
            get { return _targetCredit; }
            set { this.RaiseAndSetIfChanged(ref _targetCredit, value); }
        }
        Guid? _targetId;

        public Guid? TargetId
        {
            get { return _targetId; }
            set { this.RaiseAndSetIfChanged(ref _targetId, value); }
        }
        EntityType? _targetType;

        public EntityType? TargetType
        {
            get { return _targetType; }
            set { this.RaiseAndSetIfChanged(ref _targetType, value); }
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
        IUrl? _url;

        public IUrl? Url
        {
            get { return _url; }
            set { this.RaiseAndSetIfChanged(ref _url, value); }
        }
        IWork? _work;

        public IWork? Work
        {
            get { return _work; }
            set { this.RaiseAndSetIfChanged(ref _work, value); }
        }
        IReadOnlyDictionary<string, object?>? _unhandledProperties;

        public IReadOnlyDictionary<string, object?>? UnhandledProperties
        {
            get { return _unhandledProperties; }
            set { this.RaiseAndSetIfChanged(ref _unhandledProperties, value); }
        }

        public MusicBrainzRelationshipViewModel()
        {

        }
    }
}
