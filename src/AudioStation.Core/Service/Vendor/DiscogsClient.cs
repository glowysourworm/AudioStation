using System.Net.Http;

using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Service.Interface;

using ParkSquare.Discogs;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

using IDiscogsClient = AudioStation.Core.Service.Vendor.Interface.IDiscogsClient;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(IDiscogsClient))]
    public class DiscogsClient : IDiscogsClient
    {
        private HttpClient _httpClient;

        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationDataService, IAudioStationDataService.Status> StatusChangeEvent;

        private IAudioStationDataService.Status _status;

        public DiscogsClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<DiscogsNowPlaying> GetDiscogsNowPlaying(string artistName, string albumName)
        {
            var config = new DiscogsClientConfig()
            {
                BaseUrl = "https://api.discogs.com",
                AuthToken = "MEOjiEkEeZFdGbMnNQBvFkHKMxXHPmmaRjInFQMe"
            };

            var queryBuilder = new ApiQueryBuilder(config);
            var client = new ParkSquare.Discogs.DiscogsClient(_httpClient, queryBuilder);

            var response = await client.SearchAsync(new SearchCriteria()
            {
                Artist = artistName,
                ReleaseTitle = albumName
            });

            return null;
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "Discogs Client";
        }
        public string GetDisplayName()
        {
            return "Discogs Client";
        }
        public IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            //if (string.IsNullOrWhiteSpace(_configurationManager.GetConfiguration().AcoustIDAPIKey))
            //    return _status;


            // -> Idle
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

        public IAudioStationDataService.Status GetStatus()
        {
            return _status;
        }
        #endregion
    }
}
