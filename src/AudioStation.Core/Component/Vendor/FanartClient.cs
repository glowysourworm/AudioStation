using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component.Vendor
{
    [IocExport(typeof(IFanartClient))]
    public class FanartClient : IFanartClient
    {
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
                    ApplicationHelpers.Log("Error connecting to Fanart.tv:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);

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
                    ApplicationHelpers.Log("Error connecting to Fanart.tv:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);

                    return Enumerable.Empty<string>();
                }
            });
        }
    }
}
