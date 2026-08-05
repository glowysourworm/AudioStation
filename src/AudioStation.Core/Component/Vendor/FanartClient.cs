using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component.Vendor
{
    [IocExport(typeof(IFanartClient))]
    public class FanartClient : IFanartClient
    {
        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationComponent, IAudioStationComponent.Status> StatusChangeEvent;

        [IocImportingConstructor]
        public FanartClient(IConfigurationManager confiugrationManager)
        {
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
                    ApplicationHelpers.Log("Error connecting to Fanart.tv:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);

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
                    ApplicationHelpers.Log("Error connecting to Fanart.tv:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);

                    return Enumerable.Empty<string>();
                }
            });
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "Fanart Client";
        }
        public string GetDisplayName()
        {
            return "Fanart Client";
        }
        public IAudioStationComponent.Status GetStatus()
        {
            // TODO
            return IAudioStationComponent.Status.Idle;
        }
        public async Task<IAudioStationComponent.Status> Initialize()
        {
            // TODO
            return IAudioStationComponent.Status.Idle;
        }
        public string GetStatusMessage()
        {
            return "TODO (Fanart Client)";
        }
        #endregion
    }
}
