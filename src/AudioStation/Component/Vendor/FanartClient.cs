using System.Windows;
using System.Windows.Threading;

using AudioStation.Component.Vendor.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component.Vendor
{
    [IocExport(typeof(IFanartClient))]
    public class FanartClient : IFanartClient
    {
        private readonly IOutputController _outputController;

        [IocImportingConstructor]
        public FanartClient(IConfigurationManager confiugrationManager, IOutputController outputController)
        {
            _outputController = outputController;
        }

        public Task<IEnumerable<string>> GetArtistBackgrounds(string musicBrainzArtistId)
        {
            return Task.Run(() =>
            {
                try
                {
                    var artist = new FanartTv.Music.Artist(musicBrainzArtistId);

                    return artist.List.AImagesrtistbackground.Select(x => x.Url).ToList();
                }
                catch (Exception ex)
                {
                    RaiseLog("Error connecting to Fanart.tv:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);

                    return Enumerable.Empty<string>();
                }
            });
        }

        public Task<IEnumerable<string>> GetArtistImages(string musicBrainzArtistId)
        {
            return Task.Run(() =>
            {
                try
                {
                    var artist = new FanartTv.Music.Artist(musicBrainzArtistId);

                    return artist.List.Artistthumb.Select(x => x.Url).ToList();
                }
                catch (Exception ex)
                {
                    RaiseLog("Error connecting to Fanart.tv:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);

                    return Enumerable.Empty<string>();
                }
            });
        }

        /// <summary>
        /// Invokes logger on the application dispatcher thread
        /// </summary>
        protected void RaiseLog(string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            if (Thread.CurrentThread.ManagedThreadId != Application.Current.Dispatcher.Thread.ManagedThreadId)
                Application.Current.Dispatcher.BeginInvoke(RaiseLog, DispatcherPriority.Background, message, type, level, parameters);

            else
                _outputController.Log(message, type, level, parameters);
        }
    }
}
