using AudioStation.Core.Model;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class LibraryEntryViewModel : EntityViewModel
    {
        string _fileName;
        string _primaryArtist;
        string _primaryGenre;
        string _album;
        string _title;
        uint _track;
        uint _disc;
        TimeSpan _duration;

        bool _isFileAvailable;
        bool _isFileCorrupt;
        bool _isFileLoadError;
        string _fileLoadErrorMessage;
        string _fileCorruptMessage;

        /// <summary>
        /// File on the system for the matching database entry
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public string PrimaryArtist
        {
            get { return _primaryArtist; }
            set { this.RaiseAndSetIfChanged(ref _primaryArtist, value); }
        }
        public string PrimaryGenre
        {
            get { return _primaryGenre; }
            set { this.RaiseAndSetIfChanged(ref _primaryGenre, value); }
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
        public uint Track
        {
            get { return _track; }
            set { this.RaiseAndSetIfChanged(ref _track, value); }
        }
        public uint Disc
        {
            get { return _disc; }
            set { this.RaiseAndSetIfChanged(ref _disc, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }
        public bool IsFileAvailable
        {
            get { return _isFileAvailable; }
            set { this.RaiseAndSetIfChanged(ref _isFileAvailable, value); }
        }
        public bool IsFileCorrupt
        {
            get { return _isFileCorrupt; }
            set { this.RaiseAndSetIfChanged(ref _isFileCorrupt, value); }
        }
        public bool IsFileLoadError
        {
            get { return _isFileLoadError; }
            set { this.RaiseAndSetIfChanged(ref _isFileLoadError, value); }
        }
        public string FileLoadErrorMessage
        {
            get { return _fileLoadErrorMessage; }
            set { this.RaiseAndSetIfChanged(ref _fileLoadErrorMessage, value); }
        }
        public string FileCorruptMessage
        {
            get { return _fileCorruptMessage; }
            set { this.RaiseAndSetIfChanged(ref _fileCorruptMessage, value); }
        }

        public LibraryEntryViewModel(int id) : base(id, LibraryEntityType.Track)
        {
            this.FileName = string.Empty;
            this.Title = string.Empty;
            this.Album = string.Empty;
            this.PrimaryArtist = string.Empty;
            this.PrimaryGenre = string.Empty;
            this.Duration = TimeSpan.Zero;
            this.FileLoadErrorMessage = string.Empty;
            this.FileCorruptMessage = string.Empty;
        }
    }
}
