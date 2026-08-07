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
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        private IAudioStationService.Status _status;

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
        public async Task<IAudioStationService.Status> Initialize()
        {
            //if (string.IsNullOrWhiteSpace(_configurationManager.GetConfiguration().AcoustIDAPIKey))
            //    return _status;


            // -> Idle
            OnStatusChanged(IAudioStationService.Status.Idle);

            return _status;
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

        public IAudioStationService.Status GetStatus()
        {
            return _status;
        }
        #endregion
    }
}
