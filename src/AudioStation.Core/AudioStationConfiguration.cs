using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;

namespace AudioStation.Core
{
    public class AudioStationConfiguration : IAudioStationConfiguration
    {
        public List<LibraryDirectory> LibraryDirectories { get; set; }

        public LibraryDirectory ApplicationCacheFolder { get; set; }
        public LibraryDirectory ApplicationStorageFolder { get; set; }

        public LibraryDirectory StagingFolder { get; set; }
        public LibraryDirectory DownloadFolder { get; set; }

        public string DatabaseHost { get; set; }
        public string DatabaseName { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }

        public string BandcampEmail { get; set; }
        public string BandcampPassword { get; set; }
        public string BandcampAPIKey { get; set; }
        public string BandcampAPISecret { get; set; }

        public string LastFmUser { get; set; }
        public string LastFmPassword { get; set; }
        public string LastFmApplication { get; set; }
        public string LastFmAPIKey { get; set; }
        public string LastFmAPISecret { get; set; }
        public string LastFmAPIUser { get; set; }

        public string SpotifyClientId { get; set; }
        public string SpotifyClientSecret { get; set; }

        public string FanartUser { get; set; }
        public string FanartEmail { get; set; }
        public string FanartPassword { get; set; }
        public string FanartAPIKey { get; set; }

        public string DiscogsEmail { get; set; }
        public string DiscogsKey { get; set; }
        public string DiscogsSecret { get; set; }
        public string DiscogsCurrentToken { get; set; }

        public string MusicBrainzUser { get; set; }
        public string MusicBrainzPassword { get; set; }
        public string AcoustIDAPIKey { get; set; }

        public AudioStationConfiguration()
        {
            this.LibraryDirectories = new List<LibraryDirectory>();
            this.ApplicationCacheFolder = new LibraryDirectory()
            {
                DirectoryLabel = "Cache"
            };
            this.ApplicationStorageFolder = new LibraryDirectory()
            {
                DirectoryLabel = "Storage"
            };
            this.DownloadFolder = new LibraryDirectory()
            {
                DirectoryLabel = "Downloads"
            };
            this.StagingFolder = new LibraryDirectory()
            {
                DirectoryLabel = "Staging"
            };
        }
    }
}
