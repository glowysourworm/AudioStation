using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Component.Bandcamp.Interface;
using AudioStation.Component.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component.Bandcamp
{
    [IocExport(typeof(IBandcampClient))]
    public class BandcampClient : IBandcampClient
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IOutputController _outputController;

        [IocImportingConstructor]
        public BandcampClient(IConfigurationManager configurationManager, 
                              IOutputController outputController)
        {
            _configurationManager = configurationManager;
            _outputController = outputController;
        }

        public async Task Download(string endpoint)
        {
            try
            {
                var client = new HttpClient();
                var downloader = new BCDownloader.Downloader(client);

                var album = await downloader.GetAlbumInfoAsync(endpoint);

                if (album == null || 
                    album.TrackInfo == null || 
                    string.IsNullOrEmpty(album.Artist) ||
                    string.IsNullOrEmpty(album.Title?.Title))
                    return;

                var baseFolder = Path.Combine(_configurationManager.GetConfiguration().DownloadFolder, "Bandcamp");
                var artistFolder = Path.Combine(baseFolder, album.Artist);
                var albumFolder = Path.Combine(artistFolder, album.Title.Title);

                if (!Path.Exists(baseFolder))
                    Directory.CreateDirectory(baseFolder);

                if (!Path.Exists(artistFolder)) 
                    Directory.CreateDirectory(artistFolder);

                if (!Path.Exists(albumFolder))
                    Directory.CreateDirectory(albumFolder);

                var bmpPath = Path.Combine(albumFolder, string.Format("{0}-{1}.bmp", album.Title?.Title, album.Artist, ".bmp"));

                // Write Album Art
                if (album.CoverData != null)
                    System.IO.File.WriteAllBytes(bmpPath, album.CoverData);

                foreach (var track in album.TrackInfo)
                {
                    if (track.Data == null)
                        continue;

                    var fileFormat = "{0}-{1}-{2}.{3}";

                    var mp3Path = Path.Combine(albumFolder, string.Format(fileFormat, track.Artist, album.Title?.Title, track.Title, ".mp3"));

                    // Write Mp3 to file
                    System.IO.File.WriteAllBytes(mp3Path, track.Data);

                    RaiseLog("Successfully received album info:  {0}", LogMessageType.General, LogLevel.Information, mp3Path);
                }
            }
            catch (Exception ex)
            {
                RaiseLog("Error connecting to Bandcamp:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }
        }

        /// <summary>
        /// Invokes logger on the application dispatcher thread
        /// </summary>
        protected void RaiseLog(string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(RaiseLog, DispatcherPriority.Background, message, type, level, parameters);

            else
                _outputController.AddLog(message, type, level, parameters);
        }
    }
}
