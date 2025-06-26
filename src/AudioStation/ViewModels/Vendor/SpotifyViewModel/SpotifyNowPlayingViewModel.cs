using System.Collections.ObjectModel;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.SpotifyViewModel
{
    public class SpotifyNowPlayingViewModel : ViewModelBase
    {
        string _artistUrl;
        string _albumUrl;

        ObservableCollection<string> _albumExtenralUrls;
        ObservableCollection<string> _artistExternalUrls;

        ObservableCollection<string> _artistImages;
        ObservableCollection<string> _albumImages;

        ObservableCollection<string> _combinedImages;

        public string ArtistUrl
        {
            get { return _artistUrl; }
            set { this.RaiseAndSetIfChanged(ref _artistUrl, value); }
        }
        public string AlbumUrl
        {
            get { return _albumUrl; }
            set { this.RaiseAndSetIfChanged(ref _albumUrl, value); }
        }
        public ObservableCollection<string> AlbumExtenralUrls
        {
            get { return _albumExtenralUrls; }
            set { this.RaiseAndSetIfChanged(ref _albumExtenralUrls, value); }
        }
        public ObservableCollection<string> ArtistExternalUrls
        {
            get { return _artistExternalUrls; }
            set { this.RaiseAndSetIfChanged(ref _artistExternalUrls, value); }
        }
        public ObservableCollection<string> ArtistImages
        {
            get { return _artistImages; }
            set { this.RaiseAndSetIfChanged(ref _artistImages, value); }
        }
        public ObservableCollection<string> AlbumImages
        {
            get { return _albumImages; }
            set { this.RaiseAndSetIfChanged(ref _albumImages, value); }
        }
        public ObservableCollection<string> CombinedImages
        {
            get { return _combinedImages; }
            set { this.RaiseAndSetIfChanged(ref _combinedImages, value); }
        }

        public SpotifyNowPlayingViewModel()
        {
            this.ArtistUrl = string.Empty;
            this.AlbumUrl = string.Empty;
            this.AlbumImages = new ObservableCollection<string>();
            this.ArtistImages = new ObservableCollection<string>();
            this.CombinedImages = new ObservableCollection<string>();
            this.AlbumExtenralUrls = new ObservableCollection<string>();
            this.ArtistExternalUrls = new ObservableCollection<string>();
        }
    }
}
