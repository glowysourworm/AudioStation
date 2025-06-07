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

        public Configuration()
        {
            this.DirectoryBase = string.Empty;
            this.DatabaseHost = string.Empty;
            this.DatabaseName = string.Empty;
            this.DatabaseUser = string.Empty;
            this.DatabasePassword = string.Empty;
        }
    }
}
