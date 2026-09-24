using AudioStation.Core.Model.Interface;
using AudioStation.Core.Utility;

using SimpleWpf.UI.ViewModel;

namespace AudioStation.ViewModels.TagViewModels
{
    public class TagSmallEditViewModel : ViewModelBase, ITagSmallValidation, ITagSmall
    {
        int _id;

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

        bool _isAlbumArtistModified;
        bool _isAlbumModified;
        bool _isTitleModified;
        bool _isGenreModified;
        bool _isTrackModified;
        bool _isTrackTotalModified;
        bool _isMediaNumberModified;
        bool _isMediaTotalModified;
        bool _isMediaFormatModified;
        bool _isDurationMillisecondsModified;
        bool _isYearModified;

        string? _albumArtist;
        string? _album;
        string? _title;
        string? _genre;
        int? _trackNumber;
        int? _trackTotal;
        int? _mediaNumber;
        int? _mediaTotal;
        string? _mediaFormat;
        int? _durationMilliseconds;
        int? _year;

        bool _isValid;
        bool _isModified;
        string _validationMessage;

        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
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

        public bool IsAlbumArtistModified
        {
            get { return _isAlbumArtistModified; }
            set { this.RaiseAndSetIfChanged(ref _isAlbumArtistModified, value); }
        }
        public bool IsAlbumModified
        {
            get { return _isAlbumModified; }
            set { this.RaiseAndSetIfChanged(ref _isAlbumModified, value); }
        }
        public bool IsTitleModified
        {
            get { return _isTitleModified; }
            set { this.RaiseAndSetIfChanged(ref _isTitleModified, value); }
        }
        public bool IsGenreModified
        {
            get { return _isGenreModified; }
            set { this.RaiseAndSetIfChanged(ref _isGenreModified, value); }
        }
        public bool IsTrackModified
        {
            get { return _isTrackModified; }
            set { this.RaiseAndSetIfChanged(ref _isTrackModified, value); }
        }
        public bool IsTrackTotalModified
        {
            get { return _isTrackTotalModified; }
            set { this.RaiseAndSetIfChanged(ref _isTrackTotalModified, value); }
        }
        public bool IsMediaNumberModified
        {
            get { return _isMediaNumberModified; }
            set { this.RaiseAndSetIfChanged(ref _isMediaNumberModified, value); }
        }
        public bool IsMediaTotalModified
        {
            get { return _isMediaTotalModified; }
            set { this.RaiseAndSetIfChanged(ref _isMediaTotalModified, value); }
        }
        public bool IsMediaFormatModified
        {
            get { return _isMediaFormatModified; }
            set { this.RaiseAndSetIfChanged(ref _isMediaFormatModified, value); }
        }
        public bool IsDurationMillisecondsModified
        {
            get { return _isDurationMillisecondsModified; }
            set { this.RaiseAndSetIfChanged(ref _isDurationMillisecondsModified, value); }
        }
        public bool IsYearModified
        {
            get { return _isYearModified; }
            set { this.RaiseAndSetIfChanged(ref _isYearModified, value); }
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
        public int? Year
        {
            get { return _year; }
            set { this.RaiseAndSetIfChanged(ref _year, value); }
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

        protected override void OnPropertyChanged(string name)
        {
            // <- RaiseAndSetIfChanged
            base.OnPropertyChanged(name);

            switch (name)
            {
                case "AlbumArtist":
                    this.IsAlbumArtistModified = true;
                    break;
                case "Album":
                    this.IsAlbumModified = true;
                    break;
                case "Title":
                    this.IsTitleModified = true;
                    break;
                case "Genre":
                    this.IsGenreModified = true;
                    break;
                case "TrackNumber":
                    this.IsTrackModified = true;
                    break;
                case "TrackTotal":
                    this.IsTrackTotalModified = true;
                    break;
                case "MediaNumber":
                    this.IsMediaNumberModified = true;
                    break;
                case "MediaTotal":
                    this.IsMediaTotalModified = true;
                    break;
                case "MediaFormat":
                    this.IsMediaFormatModified = true;
                    break;
                case "DurationMilliseconds":
                    this.IsDurationMillisecondsModified = true;
                    break;
                case "Year":
                    this.IsYearModified = true;
                    break;
                default:
                    break;
            }

            // Run Validation
            OnUpdate();
        }

        protected void OnUpdate()
        {
            var validation = TagValidator.ValidateTag(this);

            this.ValidationMessage = validation.ValidationMessage;
            this.IsValid = validation.IsValid;

            this.IsAlbumValid = validation.IsAlbumValid;
            this.IsAlbumArtistValid = validation.IsAlbumArtistValid;
            this.IsGenreValid = validation.IsGenreValid;
            this.IsTitleValid = validation.IsTitleValid;
            this.IsTrackValid = validation.IsTrackValid;
            this.IsTrackTotalValid = validation.IsTrackTotalValid;
            this.IsMediaNumberValid = validation.IsMediaNumberValid;
            this.IsMediaTotalValid = validation.IsMediaTotalValid;
            this.IsMediaFormatValid = validation.IsMediaFormatValid;
            this.IsDurationMillisecondsValid = validation.IsDurationMillisecondsValid;
            this.IsYearValid = validation.IsYearValid;

            this.IsModified = this.IsAlbumModified ||
                              this.IsAlbumArtistModified ||
                              this.IsDurationMillisecondsModified ||
                              this.IsGenreModified ||
                              this.IsTitleModified ||
                              this.IsTrackModified ||
                              this.IsTrackTotalModified ||
                              this.IsMediaNumberModified ||
                              this.IsMediaTotalModified ||
                              this.IsMediaFormatModified ||
                              this.IsYearModified;
        }

        public TagSmallEditViewModel()
        {
            this.IsValid = false;
            this.IsModified = false;
            this.ValidationMessage = "Validation Not Checked!";

            OnUpdate();
        }

        public override string ToString()
        {
            return TagUtility.CreateDropdownText(this);
        }
    }
}
