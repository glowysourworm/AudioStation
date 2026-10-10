using AudioStation.Core.Model;
using AudioStation.Utility;

using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.ViewModels.LibraryViewModels
{
    /// <summary>
    /// Component used to view album details in the application (for valid library entries)
    /// </summary>
    public class AlbumViewModel : EntityViewModel
    {
        string _album;
        string _primaryArtist;
        string _mediaFormat;
        int _year;
        TimeSpan _duration;
        SortedObservableCollection<MediaViewModel> _media;

        public string Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
        public string MediaFormat
        {
            get { return _mediaFormat; }
            set { this.RaiseAndSetIfChanged(ref _mediaFormat, value); }
        }
        public string PrimaryArtist
        {
            get { return _primaryArtist; }
            set { this.RaiseAndSetIfChanged(ref _primaryArtist, value); }
        }
        public int Year
        {
            get { return _year; }
            set { this.RaiseAndSetIfChanged(ref _year, value); }
        }
        public TimeSpan Duration
        {
            get { return _duration; }
            set { this.RaiseAndSetIfChanged(ref _duration, value); }
        }
        public SortedObservableCollection<MediaViewModel> Media
        {
            get { return _media; }
            set { this.RaiseAndSetIfChanged(ref _media, value); }
        }

        public AlbumViewModel(Guid id) : base(id, LibraryEntryType.Album)
        {
            this.Media = new SortedObservableCollection<MediaViewModel>(new PropertyComparer<int, MediaViewModel>(x => x.MediaNumber));
            this.Duration = TimeSpan.Zero;
            this.Album = string.Empty;
            this.PrimaryArtist = string.Empty;
            this.Year = 0;
        }
    }
}
