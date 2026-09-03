using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(IFanartClient))]
    public class FanartClient : IFanartClient
    {
        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        private IAudioStationService.Status _status;

        [IocImportingConstructor]
        public FanartClient()
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
                    ApplicationHelpers.Log("Error connecting to Fanart.tv:  {0}", LogMessageServiceType.Fanart, LogLevel.Error, ex, ex.Message);

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
                    OnStatusChanged(IAudioStationService.Status.Working);

                    var artist = new FanartTv.Music.Artist(musicBrainzArtistId);

                    OnStatusChanged(IAudioStationService.Status.Idle);

                    return artist.List.Artistthumb.Select(x => x.Url).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error connecting to Fanart.tv:  {0}", LogMessageServiceType.Fanart, LogLevel.Error, ex, ex.Message);
                    OnStatusChanged(IAudioStationService.Status.Error);
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
        public IAudioStationService.Status GetStatus()
        {
            return _status;
        }
        public IAudioStationService.Status Initialize(AudioStationConfiguration configuration)
        {
            // No formal authentication (these keys are set in their nuget package. They should probably be substituted
            // with my API key
            //
            FanartTv.API.Key = configuration.FanartAPIKey;

            // -> Error
            if (string.IsNullOrWhiteSpace(FanartTv.API.Key))
                OnStatusChanged(IAudioStationService.Status.Error);

            // -> Idle
            else
                OnStatusChanged(IAudioStationService.Status.Idle);

            return _status;
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
            return this.GetDisplayName() + " " + IAudioStationService.GetDefaultStatusMessage(_status);
        }

        private void OnStatusChanged(IAudioStationService.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}
