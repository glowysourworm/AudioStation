using AudioStation.Core.Model;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;

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
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        private IAudioStationDataService.Status _status;

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
                    OnStatusChanged(IAudioStationDataService.Status.Working);

                    var artist = new FanartTv.Music.Artist(musicBrainzArtistId);

                    OnStatusChanged(IAudioStationDataService.Status.Idle);

                    return artist.List.Artistthumb.Select(x => x.Url).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error connecting to Fanart.tv:  {0}", LogMessageServiceType.Fanart, LogLevel.Error, ex, ex.Message);
                    OnStatusChanged(IAudioStationDataService.Status.Error);
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
        public IAudioStationDataService.Status GetStatus()
        {
            return _status;
        }
        public IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            // No formal authentication (these keys are set in their nuget package. They should probably be substituted
            // with my API key
            //
            FanartTv.API.Key = configuration.FanartAPIKey;

            // -> Error
            if (string.IsNullOrWhiteSpace(FanartTv.API.Key))
                OnStatusChanged(IAudioStationDataService.Status.Error);

            // -> Idle
            else
                OnStatusChanged(IAudioStationDataService.Status.Idle);

            return _status;
        }

        public Task<IAudioStationDataService.Status> InitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.Run(() => Initialize(configuration));
        }

        public IAudioStationDataService.Status ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationDataService.Status.Idle;
        }

        public Task<IAudioStationDataService.Status> ReInitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.FromResult(IAudioStationDataService.Status.Idle);
        }
        public string GetStatusMessage()
        {
            return this.GetDisplayName() + " " + IAudioStationDataService.GetDefaultStatusMessage(_status);
        }

        private void OnStatusChanged(IAudioStationDataService.Status status)
        {
            _status = status;

            if (this.StatusChangeEvent != null)
                this.StatusChangeEvent(this, _status);
        }
        #endregion
    }
}
