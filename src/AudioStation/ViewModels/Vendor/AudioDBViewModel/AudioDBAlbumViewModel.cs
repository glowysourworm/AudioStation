using SimpleWpf.Extensions;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.Vendor.AudioDBViewModel
{
    public class AudioDBAlbumViewModel : ViewModelBase
    {
        int _idAlbum;

        public int IdAlbum
        {
            get { return _idAlbum; }
            set { this.RaiseAndSetIfChanged(ref _idAlbum, value); }
        }
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
        string _musicBrainzArtistId;

        public string MusicBrainzArtistId
        {
            get { return _musicBrainzArtistId; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzArtistId, value); }
        }
        string _allMusicId;

        public string AllMusicId
        {
            get { return _allMusicId; }
            set { this.RaiseAndSetIfChanged(ref _allMusicId, value); }
        }
        string _bbcReviewId;

        public string BBCReviewId
        {
            get { return _bbcReviewId; }
            set { this.RaiseAndSetIfChanged(ref _bbcReviewId, value); }
        }
        string _rateYourMusicId;

        public string RateYourMusicId
        {
            get { return _rateYourMusicId; }
            set { this.RaiseAndSetIfChanged(ref _rateYourMusicId, value); }
        }
        string _discogsId;

        public string DiscogsId
        {
            get { return _discogsId; }
            set { this.RaiseAndSetIfChanged(ref _discogsId, value); }
        }
        string _wikidataId;

        public string WikidataId
        {
            get { return _wikidataId; }
            set { this.RaiseAndSetIfChanged(ref _wikidataId, value); }
        }
        string _wikipediaId;

        public string WikipediaId
        {
            get { return _wikipediaId; }
            set { this.RaiseAndSetIfChanged(ref _wikipediaId, value); }
        }
        string _geniusId;

        public string GeniusId
        {
            get { return _geniusId; }
            set { this.RaiseAndSetIfChanged(ref _geniusId, value); }
        }
        string _lyricWikiId;

        public string LyricWikiId
        {
            get { return _lyricWikiId; }
            set { this.RaiseAndSetIfChanged(ref _lyricWikiId, value); }
        }
        string _musicMozId;

        public string MusicMozId
        {
            get { return _musicMozId; }
            set { this.RaiseAndSetIfChanged(ref _musicMozId, value); }
        }
        string _itunesId;

        public string ITunesId
        {
            get { return _itunesId; }
            set { this.RaiseAndSetIfChanged(ref _itunesId, value); }
        }
        string _amazonId;

        public string AmazonId
        {
            get { return _amazonId; }
            set { this.RaiseAndSetIfChanged(ref _amazonId, value); }
        }
        int _yearReleased;

        public int YearReleased
        {
            get { return _yearReleased; }
            set { this.RaiseAndSetIfChanged(ref _yearReleased, value); }
        }
        string _albumName;

        public string AlbumName
        {
            get { return _albumName; }
            set { this.RaiseAndSetIfChanged(ref _albumName, value); }
        }
        string _albumArt;

        public string AlbumArt
        {
            get { return _albumArt; }
            set { this.RaiseAndSetIfChanged(ref _albumArt, value); }
        }
        string _albumBack;

        public string AlbumBack
        {
            get { return _albumBack; }
            set { this.RaiseAndSetIfChanged(ref _albumBack, value); }
        }
        string _album3DCase;

        public string Album3DCase
        {
            get { return _album3DCase; }
            set { this.RaiseAndSetIfChanged(ref _album3DCase, value); }
        }
        string _album3DFace;

        public string Album3DFace
        {
            get { return _album3DFace; }
            set { this.RaiseAndSetIfChanged(ref _album3DFace, value); }
        }
        string _album3DFlat;

        public string Album3DFlat
        {
            get { return _album3DFlat; }
            set { this.RaiseAndSetIfChanged(ref _album3DFlat, value); }
        }
        string _album3DThumb;

        public string Album3DThumb
        {
            get { return _album3DThumb; }
            set { this.RaiseAndSetIfChanged(ref _album3DThumb, value); }
        }
        string _albumThumb;

        public string AlbumThumb
        {
            get { return _albumThumb; }
            set { this.RaiseAndSetIfChanged(ref _albumThumb, value); }
        }
        string _albumThumbHQ;

        public string AlbumThumbHQ
        {
            get { return _albumThumbHQ; }
            set { this.RaiseAndSetIfChanged(ref _albumThumbHQ, value); }
        }
        string _description;

        public string Description
        {
            get { return _description; }
            set { this.RaiseAndSetIfChanged(ref _description, value); }
        }
    }
}
