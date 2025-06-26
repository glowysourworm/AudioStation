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

        public Configuration()
        {
            this.DirectoryBase = string.Empty;
            this.DatabaseHost = string.Empty;
            this.DatabaseName = string.Empty;
            this.DatabaseUser = string.Empty;
            this.DatabasePassword = string.Empty;
            this.BandcampEmail = string.Empty;
            this.BandcampAPIKey = string.Empty;
            this.DownloadFolder = string.Empty;

            this.LastFmAPIKey = string.Empty;
            this.LastFmAPISecret = string.Empty;
            this.LastFmAPIUser = string.Empty;
            this.LastFmApplication = string.Empty;   
            this.LastFmPassword = string.Empty; 
            this.LastFmUser = string.Empty; 
        }
    }
}
