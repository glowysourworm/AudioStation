using System.IO;
using System.Net;
using System.Net.Http;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Bandcamp.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace AudioStation.Core.Component.Vendor.Bandcamp
{
    [IocExport(typeof(IBandcampClient))]
    public class BandcampClient : IBandcampClient
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IOutputController _outputController;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IFileController _fileController;

        [IocImportingConstructor]
        public BandcampClient(IConfigurationManager configurationManager,
                              IOutputController outputController,
                              ILoggerFactory loggerFactory,
                              IFileController fileController)
        {
            _configurationManager = configurationManager;
            _outputController = outputController;
            _loggerFactory = loggerFactory;
            _fileController = fileController;
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

                var baseFolder = Path.Combine(_configurationManager.GetConfiguration().DownloadFolder, "Bandcamp");
                var artistFolder = Path.Combine(baseFolder, _fileController.MakeFriendlyPath(false, album.Artist));
                var albumFolder = Path.Combine(artistFolder, _fileController.MakeFriendlyPath(false, album.Title.Title));

                if (!Path.Exists(baseFolder))
                    Directory.CreateDirectory(baseFolder);

                if (!Path.Exists(artistFolder))
                    Directory.CreateDirectory(artistFolder);

                if (!Path.Exists(albumFolder))
                    Directory.CreateDirectory(albumFolder);

                var bmpFile = _fileController.MakeFriendlyPath(true, string.Format("{0}-{1}.bmp", album.Title?.Title, album.Artist));
                var bmpPath = Path.Combine(albumFolder, bmpFile);

                // Write Album Art
                if (album.CoverData != null)
                    File.WriteAllBytes(bmpPath, album.CoverData);

                foreach (var track in album.TrackInfo)
                {
                    if (track.Data == null || track.Data.Length == 0)
                        continue;

                    var fileFormat = "{0}-{1}-{2}.{3}";

                    var mp3File = _fileController.MakeFriendlyPath(true, string.Format(fileFormat, track.Artist, album.Title?.Title, track.Title, "mp3"));
                    var mp3Path = Path.Combine(albumFolder, mp3File);

                    // Write Mp3 to file
                    File.WriteAllBytes(mp3Path, track.Data);

                    ApplicationHelpers.Log("Successfully received album info:  {0}", LogMessageType.General, LogLevel.Information, null, mp3Path);
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error connecting to Bandcamp:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
            }
        }
    }
}
