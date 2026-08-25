using AudioStation.Core.Model.Interface;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.TagViewModels
{
    public class TagSmallViewModel : ViewModelBase, ITagSmallValidation
    {
        string _albumArtist;
        string _album;
        string _title;
        string _genre;
        int _track;
        int _trackTotal;
        int _mediaNumber;
        int _mediaTotal;
        int _year;
        string _mediaFormat;
        int _durationMilliseconds;

        bool _isAlbumArtistValid;
        bool _isAlbumValid;
        bool _isTitleValid;
        bool _isGenreValid;
        bool _isTrackValid;
        bool _isTrackTotalValid;
        bool _isMediaNumberValid;
        bool _isMediaTotalValid;
        bool _isMediaFormatValid;
        bool _isDurationMillisecondsValid;
        bool _isYearValid;

        bool _isValid;
        bool _isModified;
        string _validationMessage;

        public string AlbumArtist
        {
            get { return _albumArtist; }
            set { this.RaiseAndSetIfChanged(ref _albumArtist, value); }
        }
        public string Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
        public string Title
        {
            get { return _title; }
            set { this.RaiseAndSetIfChanged(ref _title, value); }
        }
        public string Genre
        {
            get { return _genre; }
            set { this.RaiseAndSetIfChanged(ref _genre, value); }
        }
        public int Track
        {
            get { return _track; }
            set { this.RaiseAndSetIfChanged(ref _track, value); }
        }
        public int TrackTotal
        {
            get { return _trackTotal; }
            set { this.RaiseAndSetIfChanged(ref _trackTotal, value); }
        }
        public int MediaNumber
        {
            get { return _mediaNumber; }
            set { this.RaiseAndSetIfChanged(ref _mediaNumber, value); }
        }
        public int MediaTotal
        {
            get { return _mediaTotal; }
            set { this.RaiseAndSetIfChanged(ref _mediaTotal, value); }
        }
        public string MediaFormat
        {
            get { return _mediaFormat; }
            set { this.RaiseAndSetIfChanged(ref _mediaFormat, value); }
        }
        public int DurationMilliseconds
        {
            get { return _durationMilliseconds; }
            set { this.RaiseAndSetIfChanged(ref _durationMilliseconds, value); }
        }
        public int Year
        {
            get { return _year; }
            set { this.RaiseAndSetIfChanged(ref _year, value); }
        }
        public bool IsAlbumArtistValid
        {
            get { return _isAlbumArtistValid; }
            set { this.RaiseAndSetIfChanged(ref _isAlbumArtistValid, value); }
        }
        public bool IsAlbumValid
        {
            get { return _isAlbumValid; }
            set { this.RaiseAndSetIfChanged(ref _isAlbumValid, value); }
        }
        public bool IsTitleValid
        {
            get { return _isTitleValid; }
            set { this.RaiseAndSetIfChanged(ref _isTitleValid, value); }
        }
        public bool IsGenreValid
        {
            get { return _isGenreValid; }
            set { this.RaiseAndSetIfChanged(ref _isGenreValid, value); }
        }
        public bool IsTrackValid
        {
            get { return _isTrackValid; }
            set { this.RaiseAndSetIfChanged(ref _isTrackValid, value); }
        }
        public bool IsTrackTotalValid
        {
            get { return _isTrackTotalValid; }
            set { this.RaiseAndSetIfChanged(ref _isTrackTotalValid, value); }
        }
        public bool IsMediaNumberValid
        {
            get { return _isMediaNumberValid; }
            set { this.RaiseAndSetIfChanged(ref _isMediaNumberValid, value); }
        }
        public bool IsMediaTotalValid
        {
            get { return _isMediaTotalValid; }
            set { this.RaiseAndSetIfChanged(ref _isMediaTotalValid, value); }
        }
        public bool IsMediaFormatValid
        {
            get { return _isMediaFormatValid; }
            set { this.RaiseAndSetIfChanged(ref _isMediaFormatValid, value); }
        }
        public bool IsDurationMillisecondsValid
        {
            get { return _isDurationMillisecondsValid; }
            set { this.RaiseAndSetIfChanged(ref _isDurationMillisecondsValid, value); }
        }
        public bool IsYearValid
        {
            get { return _isYearValid; }
            set { this.RaiseAndSetIfChanged(ref _isYearValid, value); }
        }
        public bool IsValid
        {
            get { return _isValid; }
            set { this.RaiseAndSetIfChanged(ref _isValid, value); }
        }
        public bool IsModified
        {
            get { return _isModified; }
            set { this.RaiseAndSetIfChanged(ref _isModified, value); }
        }
        public string ValidationMessage
        {
            get { return _validationMessage; }
            set { this.RaiseAndSetIfChanged(ref _validationMessage, value); }
        }


        /// <summary>
        /// Method meant to set data from music brainz vendor service
        /// </summary>
        public void UpdateFromMusicBrainz(MusicBrainzRecordingViewModel record)
        {
            var release = record.Releases?.FirstOrDefault();
            var media = release?.Media?.FirstOrDefault(x => x.Tracks.Any(z => z.Recording?.Id.Equals(record.Id) ?? false));
            var track = media?.Tracks.FirstOrDefault(x => x.Recording?.Id.Equals(record.Id) ?? false);

            this.AlbumArtist = record.ArtistCredit?.FirstOrDefault()?.Artist?.Name ?? string.Empty;
            this.Album = release?.Title ?? string.Empty;
            this.Title = record.Title ?? string.Empty;
            this.Genre = record.Genres?.FirstOrDefault()?.Name ?? string.Empty;
            this.Track = (track?.Position ?? 0);
            this.TrackTotal = (media?.TrackCount ?? 0);
            this.MediaNumber = ((release?.Media?.IndexOf(media) + 1) ?? 0);
            this.MediaTotal = (release?.Media?.Count ?? 0);
            this.MediaFormat = media?.Format ?? string.Empty;
            this.DurationMilliseconds = (int)(track?.Length?.TotalMilliseconds ?? 0);

            Validate();
        }

        /// <summary>
        /// Sets validation from current properties
        /// </summary>
        public void Validate()
        {
            this.IsAlbumValid = !string.IsNullOrWhiteSpace(this.Album);
            this.IsAlbumArtistValid = !string.IsNullOrWhiteSpace(this.AlbumArtist);
            this.IsTitleValid = !string.IsNullOrWhiteSpace(this.Title);
            this.IsGenreValid = !string.IsNullOrWhiteSpace(this.Genre);
            this.IsTrackValid = this.Track > 0;
            this.IsTrackTotalValid = this.TrackTotal >= this.Track && this.TrackTotal > 0;
            this.IsMediaNumberValid = this.MediaNumber > 0;
            this.IsMediaTotalValid = this.MediaTotal > 0 && this.MediaTotal >= this.MediaNumber;

            this.IsValid = this.IsAlbumValid &&
                this.IsAlbumArtistValid &&
                this.IsTitleValid &&
                this.IsGenreValid &&
                this.IsTrackValid &&
                this.IsTrackTotalValid &&
                this.IsMediaNumberValid &&
                this.IsMediaTotalValid &&
                this.IsMediaFormatValid &&
                this.IsDurationMillisecondsValid;

            this.ValidationMessage = this.IsValid ? string.Empty : "Invalid fields";
        }

        public TagSmallViewModel()
        {
            this.Album = string.Empty;
            this.AlbumArtist = string.Empty;
            this.Title = string.Empty;
            this.Genre = string.Empty;

            this.ValidationMessage = string.Empty;
        }
    }
}
