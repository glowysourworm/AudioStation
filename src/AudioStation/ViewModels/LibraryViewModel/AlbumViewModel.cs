using System.Collections.ObjectModel;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModel.LibraryViewModel
{
    /// <summary>
    /// Component used to view album details in the application (for valid library entries)
    /// </summary>
    public class AlbumViewModel : ViewModelBase
    {
        string _fileNameRef;
        string _album;
        string _artist;
        uint _year;
        TimeSpan _duration;
        ObservableCollection<TitleViewModel> _tracks;

        /// <summary>
        /// Reference to the Mp3 file. The album art is too large to pre-load. So, loading will have
        /// to be accomplished on the fly.
        /// </summary>
        public string FileNameRef
        {
            get { return _fileNameRef; }
            set { this.RaiseAndSetIfChanged(ref _fileNameRef, value); }
        }

        public string Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
        public string Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        public uint Year
        {
            get { return _year; }
            set { this.RaiseAndSetIfChanged(ref _year, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }
        public ObservableCollection<TitleViewModel> Tracks
        {
            get { return _tracks; }
            set { this.RaiseAndSetIfChanged(ref _tracks, value); }
        }

        public AlbumViewModel()
        {
            this.Tracks = new ObservableCollection<TitleViewModel>();
            this.Duration = new TimeSpan();
            this.Album = string.Empty;
            this.FileNameRef = string.Empty;
            this.Year = 0;
        }
    }
}
