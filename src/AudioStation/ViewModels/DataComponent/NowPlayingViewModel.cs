using System.Collections.ObjectModel;

using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

using SimpleWpf.IocFramework.EventAggregation;

namespace AudioStation.ViewModels.DataComponent
{
    public class NowPlayingViewModel : DataComponentViewModelBase
    {
        string _artistBio;
        string _artistSummary;
        LibraryImage _startImage;
        ObservableCollection<LibraryImage> _artistImages;
        ObservableCollection<LibraryImage> _backgroundImages;
        ObservableCollection<string> _externalLinks;

        public string ArtistBio
        {
            get { return _artistBio; }
            set { this.RaiseAndSetIfChanged(ref _artistBio, value); }
        }
        public string ArtistSummary
        {
            get { return _artistSummary; }
            set { this.RaiseAndSetIfChanged(ref _artistSummary, value); }
        }
        public LibraryImage StartImage
        {
            get { return _startImage; }
            set { this.RaiseAndSetIfChanged(ref _startImage, value); }
        }
        public ObservableCollection<LibraryImage> ArtistImages
        {
            get { return _artistImages; }
            set { this.RaiseAndSetIfChanged(ref _artistImages, value); }
        }
        public ObservableCollection<LibraryImage> BackgroundImages
        {
            get { return _backgroundImages; }
            set { this.RaiseAndSetIfChanged(ref _backgroundImages, value); }
        }
        public ObservableCollection<string> ExternalLinks
        {
            get { return _externalLinks; }
            set { this.RaiseAndSetIfChanged(ref _externalLinks, value); }
        }

        public NowPlayingViewModel(IIocEventAggregator eventAggregator) : base("Now Playing")
        {
            this.ArtistImages = new ObservableCollection<LibraryImage>();
            this.BackgroundImages = new ObservableCollection<LibraryImage>();
            this.ExternalLinks = new ObservableCollection<string>();
        }

        public override void Initialize(IAudioStationConfiguration configuration)
        {

        }

        public override void Dispose()
        {
            this.ArtistBio = string.Empty;
            this.ArtistSummary = string.Empty;
            this.StartImage = null;
            this.ArtistImages.Clear();
            this.BackgroundImages.Clear();
            this.ExternalLinks.Clear();
        }
    }
}
