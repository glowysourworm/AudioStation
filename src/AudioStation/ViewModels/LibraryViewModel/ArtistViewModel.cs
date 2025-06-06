using System.Collections.ObjectModel;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModel.LibraryViewModel
{
    /// <summary>
    /// View model for viewing LibraryEntry data by artist
    /// </summary>
    public class ArtistViewModel : ViewModelBase
    {
        string _fileName;
        string _artist;
        ObservableCollection<AlbumViewModel> _albums;

        /// <summary>
        /// Reference to the Mp3 file. The album art is too large to pre-load. So, loading will have
        /// to be accomplished on the fly.
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
            set { this.RaiseAndSetIfChanged(ref _fileName, value); }
        }
        public string Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set { this.RaiseAndSetIfChanged(ref _albums, value); }
        }

        public ArtistViewModel()
        {
            this.Artist = string.Empty;
            this.Albums = new ObservableCollection<AlbumViewModel>();
            this.FileName = string.Empty;
        }
    }
}
