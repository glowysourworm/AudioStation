using System.Net.Http;

using AudioStation.Core.Model.Vendor;

using ParkSquare.Discogs;

using IDiscogsClient = AudioStation.Core.Component.Vendor.Interface.IDiscogsClient;

namespace AudioStation.Core.Component.Vendor
{
    public class DiscogsClient : IDiscogsClient
    {
        private HttpClient _httpClient;

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
    }
}
