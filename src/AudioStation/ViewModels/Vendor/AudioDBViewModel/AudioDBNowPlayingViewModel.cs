using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.AudioDBViewModel
{
    public class AudioDBNowPlayingViewModel : ViewModelBase
    {
        private AudioDBArtistViewModel _artist;
        private AudioDBAlbumViewModel _album;

        public AudioDBArtistViewModel Artist
        {
            get { return _artist; }
            set { this.RaiseAndSetIfChanged(ref _artist, value); }
        }
        public AudioDBAlbumViewModel Album
        {
            get { return _album; }
            set { this.RaiseAndSetIfChanged(ref _album, value); }
        }
    }
}
