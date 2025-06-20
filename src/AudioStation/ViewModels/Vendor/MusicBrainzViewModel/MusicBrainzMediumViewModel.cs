using MetaBrainz.MusicBrainz.Interfaces.Entities;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.MusicBrainzViewModel
{
    public class MusicBrainzMediumViewModel : ViewModelBase, IMedium
    {
        IReadOnlyList<ITrack>? _dataTracks;

        public IReadOnlyList<ITrack>? DataTracks
        {
            get { return _dataTracks; }
            set { this.RaiseAndSetIfChanged(ref _dataTracks, value); }
        }
        IReadOnlyList<IDisc>? _discs;

        public IReadOnlyList<IDisc>? Discs
        {
            get { return _discs; }
            set { this.RaiseAndSetIfChanged(ref _discs, value); }
        }
        string? _format;

        public string? Format
        {
            get { return _format; }
            set { this.RaiseAndSetIfChanged(ref _format, value); }
        }
        Guid? _formatId;

        public Guid? FormatId
        {
            get { return _formatId; }
            set { this.RaiseAndSetIfChanged(ref _formatId, value); }
        }
        int _position;

        public int Position
        {
            get { return _position; }
            set { this.RaiseAndSetIfChanged(ref _position, value); }
        }
        ITrack? _pregap;

        public ITrack? Pregap
        {
            get { return _pregap; }
            set { this.RaiseAndSetIfChanged(ref _pregap, value); }
        }
        string? _title;

        public string? Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        int _trackCount;

        public int TrackCount
        {
            get { return _trackCount; }
            set { this.RaiseAndSetIfChanged(ref _trackCount, value); }
        }
        int? _trackOffset;

        public int? TrackOffset
        {
            get { return _trackOffset; }
            set { this.RaiseAndSetIfChanged(ref _trackOffset, value); }
        }
        IReadOnlyList<ITrack>? _tracks;

        public IReadOnlyList<ITrack>? Tracks
        {
            get { return _tracks; }
            set { this.RaiseAndSetIfChanged(ref _tracks, value); }
        }
        IReadOnlyDictionary<string, object?>? _unhandledProperties;

        public IReadOnlyDictionary<string, object?>? UnhandledProperties
        {
            get { return _unhandledProperties; }
            set { this.RaiseAndSetIfChanged(ref _unhandledProperties, value); }
        }

        public MusicBrainzMediumViewModel()
        {

        }
    }
}
