using AudioStation.Core.Model;

namespace AudioStation.ViewModels.LibraryViewModels
{
    public class TrackViewModel : EntityViewModel
    {
        Guid _albumId;
        Guid _artistId;

        string _fileName;
        string _artist;
        string _genre;
        string _album;
        string _title;
        int _trackNumber;
        int _mediaNumber;
        string _mediaFormat;
        TimeSpan _duration;

        bool _isFileAvailable;
        bool _isFileLoadError;
        string _fileLoadErrorMessage;

        /// <summary>
        /// File on the system for the matching database entry
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public Guid AlbumId
        {
            get { return _albumId; }
            private set { this.RaiseAndSetIfChanged(ref _albumId, value); }
        }
        public Guid ArtistId
        {
            get { return _artistId; }
            private set { this.RaiseAndSetIfChanged(ref _artistId, value); }
        }
        public string Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        public string Genre
        {
            get { return _genre; }
            set { this.RaiseAndSetIfChanged(ref _genre, value); }
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
        public int TrackNumber
        {
            get { return _trackNumber; }
            set { this.RaiseAndSetIfChanged(ref _trackNumber, value); }
        }
        public int MediaNumber
        {
            get { return _mediaNumber; }
            set { this.RaiseAndSetIfChanged(ref _mediaNumber, value); }
        }
        public string MediaFormat
        {
            get { return _mediaFormat; }
            set { this.RaiseAndSetIfChanged(ref _mediaFormat, value); }
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

        public TrackViewModel(Guid id, Guid albumId, Guid artistId) : base(id, LibraryEntryType.Track)
        {
            this.AlbumId = albumId;
            this.ArtistId = artistId;
            this.FileName = string.Empty;
            this.Title = string.Empty;
            this.Album = string.Empty;
            this.Artist = string.Empty;
            this.Genre = string.Empty;
            this.Duration = TimeSpan.Zero;
            this.FileLoadErrorMessage = string.Empty;
        }
    }
}
