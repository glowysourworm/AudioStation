using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    public class MusicBrainzTrackViewModel : ViewModelBase, ITrack
    {
        IReadOnlyList<INameCredit>? _artistCredit;

        public IReadOnlyList<INameCredit>? ArtistCredit
        {
            get { return _artistCredit; }
            set { this.RaiseAndSetIfChanged(ref _artistCredit, value); }
        }
        Guid _id;

        public Guid Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        TimeSpan? _length;

        public TimeSpan? Length
        {
            get { return _length; }
            set { this.RaiseAndSetIfChanged(ref _length, value); }
        }
        string? _number;

        public string? Number
        {
            get { return _number; }
            set { this.RaiseAndSetIfChanged(ref _number, value); }
        }
        int? _position;

        public int? Position
        {
            get { return _position; }
            set { this.RaiseAndSetIfChanged(ref _position, value); }
        }
        IRecording? _recording;

        public IRecording? Recording
        {
            get { return _recording; }
            set { this.RaiseAndSetIfChanged(ref _recording, value); }
        }
        string? _title;

        public string? Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        IReadOnlyDictionary<string, object?>? _unhandledProperties;

        public IReadOnlyDictionary<string, object?>? UnhandledProperties
        {
            get { return _unhandledProperties; }
            set { this.RaiseAndSetIfChanged(ref _unhandledProperties, value); }
        }

        public MusicBrainzTrackViewModel()
        {

        }
    }
}
