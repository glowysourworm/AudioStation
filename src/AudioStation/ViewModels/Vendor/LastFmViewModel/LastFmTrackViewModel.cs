using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.LastFmViewModel
{
    public class LastFmTrackViewModel : ViewModelBase
    {
        string _artistImage;

        public string ArtistImage
        {
            get { return _artistImage; }
            set { this.RaiseAndSetIfChanged(ref _artistImage, value); }
        }
        string _artistUrl;

        public string ArtistUrl
        {
            get { return _artistUrl; }
            set { this.RaiseAndSetIfChanged(ref _artistUrl, value); }
        }
        string _image;

        public string Image
        {
            get { return _image; }
            set { this.RaiseAndSetIfChanged(ref _image, value); }
        }
        string _url;

        public string Url
        {
            get { return _url; }
            set { this.RaiseAndSetIfChanged(ref _url, value); }
        }

        public LastFmTrackViewModel()
        {
            this.ArtistUrl = string.Empty;
            this.ArtistImage = string.Empty;
            this.Image = string.Empty;
            this.Url = string.Empty;
        }
    }
}
