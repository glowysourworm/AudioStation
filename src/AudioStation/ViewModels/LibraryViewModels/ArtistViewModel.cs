using AudioStation.Core.Model;
using AudioStation.ViewModels;
using AudioStation.ViewModels.LibraryViewModels.Comparer;

using SimpleWpf.Extensions.ObservableCollection;

namespace AudioStation.ViewModel.LibraryViewModels
{
    /// <summary>
    /// View model for viewing LibraryEntry data by artist
    /// </summary>
    public class ArtistViewModel : EntityViewModel
    {
        string _artist;
        SortedObservableCollection<AlbumViewModel> _albums;

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

        public ArtistViewModel(int id) : base(id, LibraryEntityType.Artist)
        {
            this.Artist = string.Empty;
            this.Albums = new SortedObservableCollection<AlbumViewModel>(new PropertyComparer<uint, AlbumViewModel>(x => x.Year));
        }
    }
}
