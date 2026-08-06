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
        private readonly IConfigurationManager _configurationManager;

        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationComponent, IAudioStationComponent.Status> StatusChangeEvent;

        private IAudioStationComponent.Status _status;

        [IocImportingConstructor]
        public FanartClient(IConfigurationManager confiugrationManager)
        {
            _configurationManager = confiugrationManager;
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
                    OnStatusChanged(IAudioStationComponent.Status.Working);

                    var artist = new FanartTv.Music.Artist(musicBrainzArtistId);

                    OnStatusChanged(IAudioStationComponent.Status.Idle);

                    return artist.List.Artistthumb.Select(x => x.Url).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error connecting to Fanart.tv:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                    OnStatusChanged(IAudioStationComponent.Status.Error);
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
            return _status;
        }
        public async Task<IAudioStationComponent.Status> Initialize()
        {
            // No formal authentication (these keys are set in their nuget package. They should probably be substituted
            // with my API key
            //
            FanartTv.API.Key = _configurationManager.GetConfiguration().FanartAPIKey;

            // -> Error
            if (string.IsNullOrWhiteSpace(FanartTv.API.Key))
                OnStatusChanged(IAudioStationComponent.Status.Error);

            // -> Idle
            else
                OnStatusChanged(IAudioStationComponent.Status.Idle);

            return _status;
        }
        public string GetStatusMessage()
        {
            return this.GetDisplayName() + " " + IAudioStationComponent.GetDefaultStatusMessage(_status);
        }

        private void OnStatusChanged(IAudioStationComponent.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}
