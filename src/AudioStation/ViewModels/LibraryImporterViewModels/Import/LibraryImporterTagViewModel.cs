using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.LibraryImporterViewModels.Import
{
    public class LibraryImporterTagViewModel : ViewModelBase, ITagValidation
    {
        bool _isAlbumArtistValid;
        bool _isAlbumValid;
        bool _isTitleValid;
        bool _isGenreValid;
        bool _isTrackValid;
        bool _isTrackTotalValid;
        bool _isDiscNumberValid;
        bool _isDiscTotalValid;

        bool _isAlbumArtistModified;
        bool _isAlbumModified;
        bool _isTitleModified;
        bool _isGenreModified;
        bool _isTrackModified;
        bool _isTrackTotalModified;
        bool _isDiscNumberModified;
        bool _isDiscTotalModified;

        string _albumArtist;
        string _album;
        string _title;
        string _genre;
        uint _track;
        uint _trackTotal;
        uint _discNumber;
        uint _discTotal;

        bool _isValid;
        bool _isModified;
        string _validationMessage;

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
        public bool IsDiscNumberValid
        {
            get { return _isDiscNumberValid; }
            set { this.RaiseAndSetIfChanged(ref _isDiscNumberValid, value); }
        }
        public bool IsDiscTotalValid
        {
            get { return _isDiscTotalValid; }
            set { this.RaiseAndSetIfChanged(ref _isDiscTotalValid, value); }
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
        public bool IsDiscNumberModified
        {
            get { return _isDiscNumberModified; }
            set { this.RaiseAndSetIfChanged(ref _isDiscNumberModified, value); }
        }
        public bool IsDiscTotalModified
        {
            get { return _isDiscTotalModified; }
            set { this.RaiseAndSetIfChanged(ref _isDiscTotalModified, value); }
        }

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
        public uint Track
        {
            get { return _track; }
            set { this.RaiseAndSetIfChanged(ref _track, value); }
        }
        public uint TrackTotal
        {
            get { return _trackTotal; }
            set { this.RaiseAndSetIfChanged(ref _trackTotal, value); }
        }
        public uint DiscNumber
        {
            get { return _discNumber; }
            set { this.RaiseAndSetIfChanged(ref _discNumber, value); }
        }
        public uint DiscTotal
        {
            get { return _discTotal; }
            set { this.RaiseAndSetIfChanged(ref _discTotal, value); }
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


        public void Update(IAudioStationTag tagClean, IAudioStationTag tagDirty, ITagValidation validation)
        {
            this.ValidationMessage = validation.ValidationMessage;
            this.IsValid = validation.IsValid;

            this.IsAlbumValid = validation.IsAlbumValid;
            this.IsAlbumArtistValid = validation.IsAlbumArtistValid;
            this.IsTitleValid = validation.IsTitleValid;
            this.IsGenreValid = validation.IsGenreValid;
            this.IsTrackValid = validation.IsTrackValid;
            this.IsTrackTotalValid = validation.IsTrackTotalValid;
            this.IsDiscNumberValid = validation.IsDiscNumberValid;
            this.IsDiscTotalValid = validation.IsDiscTotalValid;

            this.Album = tagDirty.Album;
            this.AlbumArtist = tagDirty.AlbumArtist;
            this.Title = tagDirty.Title;
            this.Genre = tagDirty.Genre;
            this.Track = tagDirty.Track;
            this.TrackTotal = tagDirty.TrackTotal;
            this.DiscNumber = tagDirty.DiscNumber;
            this.DiscTotal = tagDirty.DiscTotal;

            this.IsAlbumModified = tagDirty.Album != tagClean.Album;
            this.IsAlbumArtistModified = tagDirty.AlbumArtist != tagClean.AlbumArtist;
            this.IsTitleModified = tagDirty.Title != tagClean.Title;
            this.IsGenreModified = tagDirty.Genre != tagClean.Genre;
            this.IsTrackModified = tagDirty.Track != tagClean.Track;
            this.IsTrackTotalModified = tagDirty.TrackTotal != tagClean.TrackTotal;
            this.IsDiscNumberModified = tagDirty.DiscNumber != tagClean.DiscNumber;
            this.IsDiscTotalModified = tagDirty.DiscTotal != tagClean.DiscTotal;

            this.IsModified = this.IsAlbumModified ||
                              this.IsAlbumArtistModified ||
                              this.IsTitleModified ||
                              this.IsGenreModified ||
                              this.IsTrackModified ||
                              this.IsTrackTotalModified ||
                              this.IsDiscNumberModified ||
                              this.IsDiscTotalModified;
        }

        public LibraryImporterTagViewModel()
        {
            this.Album = string.Empty;
            this.AlbumArtist = string.Empty;
            this.Title = string.Empty;
            this.Genre = string.Empty;

            this.IsValid = false;
            this.ValidationMessage = "Validation Not Checked!";
        }
    }
}
