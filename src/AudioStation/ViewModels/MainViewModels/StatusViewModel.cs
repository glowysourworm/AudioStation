using AudioStation.ViewModels.OtherViewModels;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.ViewModel;

namespace AudioStation.ViewModels.MainViewModels
{
    [IocExportDefault]
    public class StatusViewModel : ViewModelBase
    {
        string _primaryMessage;

        StatusIconViewModel _audioPlayerStatus;

        StatusIconViewModel _audioStationDbStatus;
        StatusIconViewModel _outputControllerStatus;
        StatusIconViewModel _bandcampClient;
        StatusIconViewModel _acoustIDClient;
        StatusIconViewModel _discogsClient;
        StatusIconViewModel _fanartClient;
        StatusIconViewModel _iTunesClient;
        StatusIconViewModel _lastFmClient;
        StatusIconViewModel _musicBrainzClient;
        StatusIconViewModel _spotifyClient;

        public string PrimaryMessage
        {
            get { return _primaryMessage; }
            set { this.RaiseAndSetIfChanged(ref _primaryMessage, value); }
        }

        public StatusIconViewModel AudioPlayerStatus
        {
            get { return _audioPlayerStatus; }
            set { this.RaiseAndSetIfChanged(ref _audioPlayerStatus, value); }
        }
        public StatusIconViewModel AudioStationDbStatus
        {
            get { return _audioStationDbStatus; }
            set { this.RaiseAndSetIfChanged(ref _audioStationDbStatus, value); }
        }
        public StatusIconViewModel OutputControllerStatus
        {
            get { return _outputControllerStatus; }
            set { this.RaiseAndSetIfChanged(ref _outputControllerStatus, value); }
        }
        public StatusIconViewModel BandcampClient
        {
            get { return _bandcampClient; }
            set { this.RaiseAndSetIfChanged(ref _bandcampClient, value); }
        }
        public StatusIconViewModel AcoustIDClient
        {
            get { return _acoustIDClient; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDClient, value); }
        }
        public StatusIconViewModel DiscogsClient
        {
            get { return _discogsClient; }
            set { this.RaiseAndSetIfChanged(ref _discogsClient, value); }
        }
        public StatusIconViewModel FanartClient
        {
            get { return _fanartClient; }
            set { this.RaiseAndSetIfChanged(ref _fanartClient, value); }
        }
        public StatusIconViewModel ITunesClient
        {
            get { return _iTunesClient; }
            set { this.RaiseAndSetIfChanged(ref _iTunesClient, value); }
        }
        public StatusIconViewModel LastFmClient
        {
            get { return _lastFmClient; }
            set { this.RaiseAndSetIfChanged(ref _lastFmClient, value); }
        }
        public StatusIconViewModel MusicBrainzClient
        {
            get { return _musicBrainzClient; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzClient, value); }
        }
        public StatusIconViewModel SpotifyClient
        {
            get { return _spotifyClient; }
            set { this.RaiseAndSetIfChanged(ref _spotifyClient, value); }
        }

        [IocImportingConstructor]
        public StatusViewModel()
        {
            this.PrimaryMessage = string.Empty;

            this.AudioPlayerStatus = new StatusIconViewModel();
            this.AudioStationDbStatus = new StatusIconViewModel();
            this.OutputControllerStatus = new StatusIconViewModel();
            this.BandcampClient = new StatusIconViewModel();
            this.AcoustIDClient = new StatusIconViewModel();
            this.DiscogsClient = new StatusIconViewModel();
            this.FanartClient = new StatusIconViewModel();
            this.ITunesClient = new StatusIconViewModel();
            this.LastFmClient = new StatusIconViewModel();
            this.MusicBrainzClient = new StatusIconViewModel();
            this.SpotifyClient = new StatusIconViewModel();
        }
    }
}
