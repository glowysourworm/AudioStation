using SimpleWpf.Extensions;

namespace AudioStation.Core
{
    public class Configuration : ViewModelBase
    {
        string _directoryBase;
        string _databaseHost;
        string _databaseName;
        string _databaseUser;
        string _databasePassword;

        string _musicBrainzDatabaseHost;
        string _musicBrainzDatabaseName;
        string _musicBrainzDatabaseUser;
        string _musicBrainzDatabasePassword;

        string _downloadFolder;

        string _bandcampEmail;
        string _bandcampAPIKey;

        string _lastFmUser;
        string _lastFmPassword;
        string _lastFmApplication;
        string _lastFmAPIKey;
        string _lastFmAPISecret;
        string _lastFmAPIUser;

        string _spotifyClientId;
        string _spotifyClientSecret;

        string _fanartUser;
        string _fanartEmail;
        string _fanartPassword;
        string _fanartAPIKey;

        string _discogsEmail;
        string _discogsKey;
        string _discogsSecret;
        string _discogsCurrentToken;

        string _musicBrainzUser;
        string _musicBrainzPassword;

        string _acoustIDAPIKey;

        public string DirectoryBase
        {
            get { return _directoryBase; }
            set { RaiseAndSetIfChanged(ref _directoryBase, value); }
        }

        public string DatabaseHost
        {
            get { return _databaseHost; }
            set { RaiseAndSetIfChanged(ref _databaseHost, value); }
        }
        public string DatabaseName
        {
            get { return _databaseName; }
            set { RaiseAndSetIfChanged(ref _databaseName, value); }
        }
        public string DatabaseUser
        {
            get { return _databaseUser; }
            set { RaiseAndSetIfChanged(ref _databaseUser, value); }
        }
        public string DatabasePassword
        {
            get { return _databasePassword; }
            set { RaiseAndSetIfChanged(ref _databasePassword, value); }
        }

        public string MusicBrainzDatabaseHost
        {
            get { return _musicBrainzDatabaseHost; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzDatabaseHost, value); }
        }
        public string MusicBrainzDatabaseName
        {
            get { return _musicBrainzDatabaseName; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzDatabaseName, value); }
        }
        public string MusicBrainzDatabaseUser
        {
            get { return _musicBrainzDatabaseUser; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzDatabaseUser, value); }
        }
        public string MusicBrainzDatabasePassword
        {
            get { return _musicBrainzDatabasePassword; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzDatabasePassword, value); }
        }

        public string BandcampEmail
        {
            get { return _bandcampEmail; }
            set { this.RaiseAndSetIfChanged(ref _bandcampEmail, value); }
        }
        public string BandcampAPIKey
        {
            get { return _bandcampAPIKey; }
            set { this.RaiseAndSetIfChanged(ref _bandcampAPIKey, value); }
        }
        public string DownloadFolder
        {
            get { return _downloadFolder; }
            set { this.RaiseAndSetIfChanged(ref _downloadFolder, value); }
        }

        public string LastFmUser
        {
            get { return _lastFmUser; }
            set { this.RaiseAndSetIfChanged(ref _lastFmUser, value); }
        }
        public string LastFmPassword
        {
            get { return _lastFmPassword; }
            set { this.RaiseAndSetIfChanged(ref _lastFmPassword, value); }
        }
        public string LastFmApplication
        {
            get { return _lastFmApplication; }
            set { this.RaiseAndSetIfChanged(ref _lastFmApplication, value); }
        }
        public string LastFmAPIKey
        {
            get { return _lastFmAPIKey; }
            set { this.RaiseAndSetIfChanged(ref _lastFmAPIKey, value); }
        }
        public string LastFmAPISecret
        {
            get { return _lastFmAPISecret; }
            set { this.RaiseAndSetIfChanged(ref _lastFmAPISecret, value); }
        }
        public string LastFmAPIUser
        {
            get { return _lastFmAPIUser; }
            set { this.RaiseAndSetIfChanged(ref _lastFmAPIUser, value); }
        }

        public string SpotifyClientId
        {
            get { return _spotifyClientId; }
            set { this.RaiseAndSetIfChanged(ref _spotifyClientId, value); }
        }
        public string SpotifyClientSecret
        {
            get { return _spotifyClientSecret; }
            set { this.RaiseAndSetIfChanged(ref _spotifyClientSecret, value); }
        }

        public string FanartUser
        {
            get { return _fanartUser; }
            set { this.RaiseAndSetIfChanged(ref _fanartUser, value); }
        }
        public string FanartEmail
        {
            get { return _fanartEmail; }
            set { this.RaiseAndSetIfChanged(ref _fanartEmail, value); }
        }
        public string FanartPassword
        {
            get { return _fanartPassword; }
            set { this.RaiseAndSetIfChanged(ref _fanartPassword, value); }
        }
        public string FanartAPIKey
        {
            get { return _fanartAPIKey; }
            set { this.RaiseAndSetIfChanged(ref _fanartAPIKey, value); }
        }

        public string DiscogsEmail
        {
            get { return _discogsEmail; }
            set { this.RaiseAndSetIfChanged(ref _discogsEmail, value); }
        }
        public string DiscogsKey
        {
            get { return _discogsKey; }
            set { this.RaiseAndSetIfChanged(ref _discogsKey, value); }
        }
        public string DiscogsSecret
        {
            get { return _discogsSecret; }
            set { this.RaiseAndSetIfChanged(ref _discogsSecret, value); }
        }
        public string DiscogsCurrentToken
        {
            get { return _discogsCurrentToken; }
            set { this.RaiseAndSetIfChanged(ref _discogsCurrentToken, value); }
        }

        public string MusicBrainzUser
        {
            get { return _musicBrainzUser; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzUser, value); }
        }
        public string MusicBrainzPassword
        {
            get { return _musicBrainzPassword; }
            set { this.RaiseAndSetIfChanged(ref _musicBrainzPassword, value); }
        }
        public string AcoustIDAPIKey
        {
            get { return _acoustIDAPIKey; }
            set { this.RaiseAndSetIfChanged(ref _acoustIDAPIKey, value); }
        }

        public Configuration()
        {
            this.DirectoryBase = string.Empty;

            this.DatabaseHost = string.Empty;
            this.DatabaseName = string.Empty;
            this.DatabaseUser = string.Empty;
            this.DatabasePassword = string.Empty;

            this.MusicBrainzDatabaseHost = string.Empty;
            this.MusicBrainzDatabaseName = string.Empty;
            this.MusicBrainzDatabasePassword = string.Empty;
            this.MusicBrainzDatabaseUser = string.Empty;

            this.BandcampEmail = string.Empty;
            this.BandcampAPIKey = string.Empty;
            this.DownloadFolder = string.Empty;

            this.LastFmAPIKey = string.Empty;
            this.LastFmAPISecret = string.Empty;
            this.LastFmAPIUser = string.Empty;
            this.LastFmApplication = string.Empty;   
            this.LastFmPassword = string.Empty; 
            this.LastFmUser = string.Empty;

            this.SpotifyClientId = string.Empty;
            this.SpotifyClientSecret = string.Empty;

            this.FanartAPIKey = string.Empty;
            this.FanartEmail = string.Empty;
            this.FanartPassword = string.Empty;
            this.FanartUser = string.Empty;

            this.DiscogsCurrentToken = string.Empty;
            this.DiscogsEmail = string.Empty;
            this.DiscogsKey = string.Empty;
            this.DiscogsSecret = string.Empty;

            this.MusicBrainzUser = string.Empty;
            this.MusicBrainzPassword = string.Empty;
            this.AcoustIDAPIKey = string.Empty;
        }
    }
}
