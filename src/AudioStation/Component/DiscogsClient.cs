using System.Net.Http;

using AudioStation.ViewModels.Vendor.DiscogsViewModel;

using ParkSquare.Discogs;

using SimpleWpf.Extensions;

using static System.Net.WebRequestMethods;

using IDiscogsClient = AudioStation.Component.Interface.IDiscogsClient;

namespace AudioStation.Component
{
    public class DiscogsClient : IDiscogsClient
    {
        private HttpClient _httpClient;

        public DiscogsClient()
        {
            _httpClient = new HttpClient();
        }

        public async Task<DiscogsNowPlayingViewModel> GetDiscogsNowPlaying(string artistName, string albumName)
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
