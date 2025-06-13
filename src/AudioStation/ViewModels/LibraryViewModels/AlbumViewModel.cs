using AudioStation.Core.Model;
using AudioStation.ViewModels.LibraryViewModels.Comparer;

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
        uint _year;
        TimeSpan _duration;
        SortedObservableCollection<LibraryEntryViewModel> _tracks;

        public string Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
        public string PrimaryArtist
        {
            get { return _primaryArtist; }
            set { this.RaiseAndSetIfChanged(ref _primaryArtist, value); }
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
        public SortedObservableCollection<LibraryEntryViewModel> Tracks
        {
            get { return _tracks; }
            set { this.RaiseAndSetIfChanged(ref _tracks, value); }
        }

        public AlbumViewModel(int id) : base(id, LibraryEntityType.Album)
        {
            this.Tracks = new SortedObservableCollection<LibraryEntryViewModel>(new PropertyComparer<uint, LibraryEntryViewModel>(x => x.Track));
            this.Duration = TimeSpan.Zero;
            this.Album = string.Empty;
            this.PrimaryArtist = string.Empty;
            this.Year = 0;
        }
    }
}
