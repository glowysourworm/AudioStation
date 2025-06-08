using AudioStation.ViewModels.LibraryViewModels.Comparer;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.ViewModel.LibraryViewModels
{
    /// <summary>
    /// View model for viewing LibraryEntry data by artist
    /// </summary>
    public class ArtistViewModel : ViewModelBase
    {
        int _id;
        string _artist;
        SortedObservableCollection<AlbumViewModel> _albums;

        public int Id
        {
            get { return _id; }
            set { this.RaiseAndSetIfChanged(ref _id, value); }
        }
        public string Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        public SortedObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set { this.RaiseAndSetIfChanged(ref _albums, value); }
        }

        public ArtistViewModel()
        {
            this.Artist = string.Empty;
            this.Albums = new SortedObservableCollection<AlbumViewModel>(new PropertyComparer<uint, AlbumViewModel>(x => x.Year));
        }
    }
}
