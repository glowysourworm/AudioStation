using System;

using AudioStation.Model.Comparer;
using AudioStation.ViewModels;

using SimpleWpf.Extensions.ObservableCollection;

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
        SortedObservableCollection<TitleViewModel> _tracks;

        /// <summary>
        /// Reference to the Mp3 file. The album art is too large to pre-load. So, loading will have
        /// to be accomplished on the fly.
        /// </summary>
        public string FileNameRef
        {
            get { return _fileNameRef; }
            set { this.SetProperty(ref _fileNameRef, value); }
        }

        public string Album
        {
            get { return _album; }
            set { this.SetProperty(ref _album, value); }
        }
        public string Artist
        {
            get { return _artist; }
            set { this.SetProperty(ref _artist, value); }
        }
        public uint Year
        {
            get { return _year; }
            set { this.SetProperty(ref _year, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.SetProperty(ref _duration, value); }
        }
        public SortedObservableCollection<TitleViewModel> Tracks
        {
            get { return _tracks; }
            set { this.SetProperty(ref _tracks, value); }
        }

        public AlbumViewModel()
        {
            this.Album = string.Empty;
            this.FileNameRef = string.Empty;
            this.Duration = new TimeSpan();
            this.Year = 0;
            this.Tracks = new SortedObservableCollection<TitleViewModel>(new TrackNumberComparer());
        }
    }
}
