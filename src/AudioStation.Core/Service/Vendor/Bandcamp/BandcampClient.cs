using System.IO;

using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Bandcamp.Interface;
using AudioStation.Core.Utility;
using AudioStation.Core.Utility.FileUtility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Service.Vendor.Bandcamp
{
    [IocExport(typeof(IBandcampClient))]
    public class BandcampClient : IBandcampClient
    {
        AudioStationConfiguration _configuration;

        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        [IocImportingConstructor]
        public BandcampClient()
        {
        }

        //public async Task Download(string artist)
        //{
        //    var config = _configurationManager.GetConfiguration();

        //    using (var client = new BandcampHttpClient(config.BandcampEmail, 
        //                                               config.BandcampEmail, 
        //                                               config.BandcampAPIKey, 
        //                                               config.BandcampAPISecret,
        //                                               _loggerFactory))
        //    {
        //        var collection = client.GetCollection();

        //        if (collection == null)
        //            return;

        //        await foreach (var album in collection)
        //        {
        //            var baseFolder = Path.Combine(config.DownloadFolder, "Bandcamp");
        //            var artistFolder = Path.Combine(baseFolder, StringHelpers.MakeFriendlyPath(album.Artist));
        //            var albumFolder = Path.Combine(artistFolder, StringHelpers.MakeFriendlyPath(album.Title));

        //            if (!Path.Exists(baseFolder))
        //                Directory.CreateDirectory(baseFolder);

        //            if (!Path.Exists(artistFolder))
        //                Directory.CreateDirectory(artistFolder);

        //            if (!Path.Exists(albumFolder))
        //                Directory.CreateDirectory(albumFolder);

        //            foreach (var track in album.Tracks)
        //            {
        //                var fileFormat = "{0}-{1}-{2}.{3}";

        //                var mp3Path = StringHelpers.MakeFriendlyFileName(Path.Combine(albumFolder, string.Format(fileFormat, album.Artist, album.Title, track.Title, "mp3")));
        //                var mp3Data = await client.GetAudioData(track);

        //                File.WriteAllBytes(mp3Path, mp3Data);
        //            }
        //        }
        //    }
        //}

        public async Task Download(string endpoint)
        {
            try
            {
                var client = new BandcampClientCore();
                var album = await client.GetAlbumInfoAsync(endpoint, 1000);

                if (album == null ||
                    album.TrackInfo == null ||
                    string.IsNullOrWhiteSpace(album.Artist) ||
                    string.IsNullOrWhiteSpace(album.Title?.Title))
                    throw new Exception("Error reading data from Bandcamp API. Invalid or incomplete data set.");

                var baseFolder = Path.Combine(_configuration.DownloadFolder.Directory, "Bandcamp");
                var artistFolder = Path.Combine(baseFolder, MigrationHelpers.MakeFriendlyPath(false, album.Artist));
                var albumFolder = Path.Combine(artistFolder, MigrationHelpers.MakeFriendlyPath(false, album.Title.Title));

                if (!Path.Exists(baseFolder))
                    Directory.CreateDirectory(baseFolder);

                if (!Path.Exists(artistFolder))
                    Directory.CreateDirectory(artistFolder);

                if (!Path.Exists(albumFolder))
                    Directory.CreateDirectory(albumFolder);

                var bmpFile = MigrationHelpers.MakeFriendlyPath(true, string.Format("{0}-{1}.bmp", album.Title?.Title, album.Artist));
                var bmpPath = Path.Combine(albumFolder, bmpFile);

                // Write Album Art
                if (album.CoverData != null)
                    File.WriteAllBytes(bmpPath, album.CoverData);

                foreach (var track in album.TrackInfo)
                {
                    if (track.Data == null || track.Data.Length == 0)
                        continue;

                    var fileFormat = "{0}-{1}-{2}.{3}";

                    var mp3File = MigrationHelpers.MakeFriendlyPath(true, string.Format(fileFormat, track.Artist, album.Title?.Title, track.Title, "mp3"));
                    var mp3Path = Path.Combine(albumFolder, mp3File);

                    // Write Mp3 to file
                    File.WriteAllBytes(mp3Path, track.Data);

                    ApplicationHelpers.Log("Successfully received album info:  {0}", LogMessageServiceType.Bandcamp, LogLevel.Information, null, mp3Path);
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error connecting to Bandcamp:  {0}", LogMessageServiceType.Bandcamp, LogLevel.Error, ex, ex.Message);
            }
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "Bandcamp Client";
        }
        public string GetDisplayName()
        {
            return "Bandcamp Client";
        }
        public IAudioStationService.Status GetStatus()
        {
            // TODO
            return IAudioStationService.Status.Idle;
        }
        public IAudioStationService.Status Initialize(AudioStationConfiguration configuration)
        {
            _configuration = configuration;

            return IAudioStationService.Status.Idle;
        }

        public Task<IAudioStationService.Status> InitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.Run(() => Initialize(configuration));
        }

        public IAudioStationService.Status ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationService.Status.Idle;
        }

        public Task<IAudioStationService.Status> ReInitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.FromResult(IAudioStationService.Status.Idle);
        }
        public string GetStatusMessage()
        {
            return "TODO (Bandcamp Client)";
        }
        #endregion
    }
}
