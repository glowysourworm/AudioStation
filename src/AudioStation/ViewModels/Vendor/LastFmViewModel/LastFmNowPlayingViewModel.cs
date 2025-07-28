using System.Collections.ObjectModel;

using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Vendor.LastFmViewModel
{
    public class LastFmNowPlayingViewModel : ViewModelBase
    {
        int _artistYearFormed;
        string _bioSummary;
        string _bioContent;
        string _artistUrl;
        string _artistMainImage;
        string _albumImage;
        string _albumUrl;
        ObservableCollection<LastFmTrackViewModel> _tracks;

        public int ArtistYearFormed
        {
            get { return _artistYearFormed; }
            set { this.RaiseAndSetIfChanged(ref _artistYearFormed, value); }
        }
        public string BioSummary
        {
            get { return _bioSummary; }
            set { this.RaiseAndSetIfChanged(ref _bioSummary, value); }
        }
        public string BioContent
        {
            get { return _bioContent; }
            set { this.RaiseAndSetIfChanged(ref _bioContent, value); }
        }
        public string ArtistUrl
        {
            get { return _artistUrl; }
            set { this.RaiseAndSetIfChanged(ref _artistUrl, value); }
        }
        public string ArtistMainImage
        {
            get { return _artistMainImage; }
            set { this.RaiseAndSetIfChanged(ref _artistMainImage, value); }
        }
        public string AlbumImage
        {
            get { return _albumImage; }
            set { this.RaiseAndSetIfChanged(ref _albumImage, value); }
        }
        public string AlbumUrl
        {
            get { return _albumUrl; }
            set { this.RaiseAndSetIfChanged(ref _albumUrl, value); }
        }
        public ObservableCollection<LastFmTrackViewModel> Tracks
        {
            get { return _tracks; }
            set { this.RaiseAndSetIfChanged(ref _tracks, value); }
        }


        public LastFmNowPlayingViewModel()
        {
            this.BioSummary = string.Empty;
            this.BioContent = string.Empty;
            this.ArtistUrl = string.Empty;
            this.ArtistMainImage = string.Empty;
            this.AlbumImage = string.Empty;
            this.AlbumUrl = string.Empty;
        }
    }
}
