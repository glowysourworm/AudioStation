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
        }
    }
}
