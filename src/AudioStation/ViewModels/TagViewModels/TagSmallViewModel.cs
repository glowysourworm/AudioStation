using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.TagViewModels
{
    public class TagSmallViewModel : ViewModelBase, ITagSmall
    {
        int _id;

        string? _albumArtist;
        string? _album;
        string? _title;
        string? _genre;
        int? _trackNumber;
        int? _trackTotal;
        int? _mediaNumber;
        int? _mediaTotal;
        int? _year;
        string? _mediaFormat;
        int? _durationMilliseconds;

        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string? AlbumArtist
        {
            get { return _albumArtist; }
            set { this.RaiseAndSetIfChanged(ref _albumArtist, value); }
        }
        public string? Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
        public string? Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        public string? Genre
        {
            get { return _genre; }
            set { this.RaiseAndSetIfChanged(ref _genre, value); }
        }
        public int? TrackNumber
        {
            get { return _trackNumber; }
            set { this.RaiseAndSetIfChanged(ref _trackNumber, value); }
        }
        public int? TrackTotal
        {
            get { return _trackTotal; }
            set { this.RaiseAndSetIfChanged(ref _trackTotal, value); }
        }
        public int? MediaNumber
        {
            get { return _mediaNumber; }
            set { this.RaiseAndSetIfChanged(ref _mediaNumber, value); }
        }
        public int? MediaTotal
        {
            get { return _mediaTotal; }
            set { this.RaiseAndSetIfChanged(ref _mediaTotal, value); }
        }
        public int? Year
        {
            get { return _year; }
            set { this.RaiseAndSetIfChanged(ref _year, value); }
        }
        public string? MediaFormat
        {
            get { return _mediaFormat; }
            set { this.RaiseAndSetIfChanged(ref _mediaFormat, value); }
        }
        public int? DurationMilliseconds
        {
            get { return _durationMilliseconds; }
            set { this.RaiseAndSetIfChanged(ref _durationMilliseconds, value); }
        }

        public TagSmallViewModel()
        {
        }

        public override string ToString()
        {
            return TagUtility.CreateDropdownText(this);
        }
    }
}
