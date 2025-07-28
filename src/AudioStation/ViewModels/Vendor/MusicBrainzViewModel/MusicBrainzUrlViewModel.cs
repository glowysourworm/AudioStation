using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    public class MusicBrainzUrlViewModel : ViewModelBase, IUrl
    {
        Uri? _resource;

        public Uri? Resource
        {
            get { return _resource; }
            set { this.RaiseAndSetIfChanged(ref _resource, value); }
        }
        IReadOnlyList<IRelationship>? _relationships;

        public IReadOnlyList<IRelationship>? Relationships
        {
            get { return _relationships; }
            set { this.RaiseAndSetIfChanged(ref _relationships, value); }
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

        public MusicBrainzUrlViewModel()
        {

        }
    }
}
