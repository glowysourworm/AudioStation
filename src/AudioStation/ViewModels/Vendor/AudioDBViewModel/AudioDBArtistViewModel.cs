using System.Collections.ObjectModel;

using SimpleWpf.Extensions;

namespace AudioStation.ViewModels.Vendor.AudioDBViewModel
{
    public class AudioDBArtistViewModel : ViewModelBase
    {
        ObservableCollection<AudioDBAlbumViewModel> _albums;

        int _idArtist;

        public int IdArtist
        {
            get { return _idArtist; }
            set { this.RaiseAndSetIfChanged(ref _idArtist, value); }
        }
        int _idLabel;

        public int IdLabel
        {
            get { return _idLabel; }
            set { this.RaiseAndSetIfChanged(ref _idLabel, value); }
        }
        string _musicBrainzId;

        public string MusicBrainzId
        {
            get { return _musicBrainzId; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzId, value); }
        }
        int _yearFormed;

        public int YearFormed
        {
            get { return _yearFormed; }
            set { this.RaiseAndSetIfChanged(ref _yearFormed, value); }
        }
        string _artistBio;

        public string ArtistBio
        {
            get { return _artistBio; }
            set { this.RaiseAndSetIfChanged(ref _artistBio, value); }
        }
        string _artistName;

        public string ArtistName
        {
            get { return _artistName; }
            set { this.RaiseAndSetIfChanged(ref _artistName, value); }
        }
        string _artistThumb;

        public string ArtistThumb
        {
            get { return _artistThumb; }
            set { this.RaiseAndSetIfChanged(ref _artistThumb, value); }
        }
        string _artistLogo;

        public string ArtistLogo
        {
            get { return _artistLogo; }
            set { this.RaiseAndSetIfChanged(ref _artistLogo, value); }
        }
        string _artistCutout;

        public string ArtistCutout
        {
            get { return _artistCutout; }
            set { this.RaiseAndSetIfChanged(ref _artistCutout, value); }
        }
        string _artistClearart;

        public string ArtistClearart
        {
            get { return _artistClearart; }
            set { this.RaiseAndSetIfChanged(ref _artistClearart, value); }
        }
        string _artistWideThumb;

        public string ArtistWideThumb
        {
            get { return _artistWideThumb; }
            set { this.RaiseAndSetIfChanged(ref _artistWideThumb, value); }
        }
        string _artistFanart;

        public string ArtistFanart
        {
            get { return _artistFanart; }
            set { this.RaiseAndSetIfChanged(ref _artistFanart, value); }
        }
        string _artistFanart2;

        public string ArtistFanart2
        {
            get { return _artistFanart2; }
            set { this.RaiseAndSetIfChanged(ref _artistFanart2, value); }
        }
        string _artistFanart3;

        public string ArtistFanart3
        {
            get { return _artistFanart3; }
            set { this.RaiseAndSetIfChanged(ref _artistFanart3, value); }
        }
        string _artistFanart4;

        public string ArtistFanart4
        {
            get { return _artistFanart4; }
            set { this.RaiseAndSetIfChanged(ref _artistFanart4, value); }
        }
        string _artistBanner;

        public string ArtistBanner
        {
            get { return _artistBanner; }
            set { this.RaiseAndSetIfChanged(ref _artistBanner, value); }
        }
        string _website;

        public string Website
        {
            get { return _website; }
            set { this.RaiseAndSetIfChanged(ref _website, value); }
        }
        string _facebook;

        public string Facebook
        {
            get { return _facebook; }
            set { this.RaiseAndSetIfChanged(ref _facebook, value); }
        }
        string _country;

        public string Country
        {
            get { return _country; }
            set { this.RaiseAndSetIfChanged(ref _country, value); }
        }

        public ObservableCollection<AudioDBAlbumViewModel> Albums
        {
            get { return _albums; }
            set { this.RaiseAndSetIfChanged(ref _albums, value); }
        }

        public AudioDBArtistViewModel()
        {
            this.Albums = new ObservableCollection<AudioDBAlbumViewModel>();
        }
    }
}
