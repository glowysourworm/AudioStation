using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.LibraryViewModel
{
    public class LibraryEntryViewModel : ViewModelBase
    {
        int _id;
        string _fileName;
        string _primaryArtist;
        string _primaryGenre;
        string _album;
        string _title;
        uint _track;
        uint _disc;
        bool _loadError;
        string _loadErrorMessage;

        /// <summary>
        /// Database id for the entry
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }

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
        public bool LoadError
        {
            get { return _loadError; }
            set { this.RaiseAndSetIfChanged(ref _loadError, value); }
        }
        public string LoadErrorMessage
        {
            get { return _loadErrorMessage; }
            set { this.RaiseAndSetIfChanged(ref _loadErrorMessage, value); }
        }

        public LibraryEntryViewModel()
        {
            this.FileName = string.Empty;
            this.Title = string.Empty;
            this.Album = string.Empty;
            this.PrimaryArtist = string.Empty;
            this.PrimaryGenre = string.Empty;
            this.LoadError = false;
            this.LoadErrorMessage = string.Empty;
        }
    }
}
