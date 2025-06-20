using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    public class MusicBrainzDiscViewModel : ViewModelBase, IDisc
    {
        string _id;

        public string Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        IReadOnlyList<int> _offsets;

        public IReadOnlyList<int> Offsets
        {
            get { return _offsets; }
            set { this.RaiseAndSetIfChanged(ref _offsets, value); }
        }
        IReadOnlyList<IRelease>? _releases;

        public IReadOnlyList<IRelease>? Releases
        {
            get { return _releases; }
            set { this.RaiseAndSetIfChanged(ref _releases, value); }
        }
        int _sectors;

        public int Sectors
        {
            get { return _sectors; }
            set { this.RaiseAndSetIfChanged(ref _sectors, value); }
        }
        IReadOnlyDictionary<string, object?>? _unhandledProperties;

        public IReadOnlyDictionary<string, object?>? UnhandledProperties
        {
            get { return _unhandledProperties; }
            set { this.RaiseAndSetIfChanged(ref _unhandledProperties, value); }
        }

        public MusicBrainzDiscViewModel()
        {

        }
    }
}
