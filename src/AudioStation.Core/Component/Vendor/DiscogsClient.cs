using System.Net.Http;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Model.Vendor;

using ParkSquare.Discogs;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

using IDiscogsClient = AudioStation.Core.Component.Vendor.Interface.IDiscogsClient;

namespace AudioStation.Core.Component.Vendor
{
    [IocExport(typeof(IDiscogsClient))]
    public class DiscogsClient : IDiscogsClient
    {
        private HttpClient _httpClient;

        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationComponent, IAudioStationComponent.Status> StatusChangeEvent;

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
            return "TODO (Discogs Client)";
        }
        #endregion
    }
}
