﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

using AudioStation.Core.Component.Vendor.Bandcamp.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;

namespace AudioStation.Core.Component.Vendor.Bandcamp
{
    public class BandcampHttpClient : IDisposable
    {
        private const string RequestedWith = "XMLHttpRequest";
        //private const string RequestedWith = "com.bandcamp.android";
        //private const string ClientId = "134";
        //private const string ClientSecret = "1myK12VeCL3dWl9o/ncV2VyUUbOJuNPVJK6bZZJxHvk=";
        private const string XBandcampDm = "X-Bandcamp-Dm";
        private const string XBandcampPow = "X-Bandcamp-Pow";

        // Allow one 418 and one 451 before successful response
        private const int MaxLoginAttempts = 2;
        private const int PageSize = 200;

        private static readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        private readonly ILogger<BandcampHttpClient> logger;
        private readonly HttpClientHandler handler;
        private readonly HttpClient client;
        private readonly Task<LoginResponse> loginTask;

        private string dm;
        private string pow;

        public BandcampHttpClient(string username, string password, string clientId, string clientSecret, ILoggerFactory loggerFactory)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(username);
            ArgumentNullException.ThrowIfNullOrEmpty(password);

            logger = loggerFactory.CreateLogger<BandcampHttpClient>();

            handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://bandcamp.com/")
            };

            loginTask = Login(username, password, clientId, clientSecret);
        }

        public async Task<Album> GetAlbum(long id)
        {
            var queryString = new Dictionary<string, string>
            {
                ["tralbum_type"] = "a",
                ["tralbum_id"] = id.ToString()
            };

            var collection = await GetCollection(queryString);
            return collection.Items.SingleOrDefault();
        }

        public async Task<AlbumDetails> GetAlbumDetails(long id)
        {
            var request = new AlbumDetailsRequest
            {
                TralbumType = "a",
                BandId = 1,
                TralbumId = id
            };

            using var content = JsonContent.Create(request, options: serializerOptions);

            using var response = await client.PostAsJsonAsync(
                "/api/mobile/25/tralbum_details",
                request,
                serializerOptions
            );

            return await response.Content.ReadFromJsonAsync<AlbumDetails>(serializerOptions);
        }
        
        public async Task<byte[]> GetAudioData(Track track)
        {
            using (var response = await client.GetAsync(track.HqAudioUrl))
            {
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        public async IAsyncEnumerable<Album> GetCollection()
        {
            var queryString = new Dictionary<string, string>
            {
                ["page_size"] = PageSize.ToString()
            };

            for (; ; )
            {
                var collection = await GetCollection(queryString);

                if (collection.Items.Count == 0)
                {
                    yield break;
                }

                queryString["offset"] = collection.Items.Last().Token;

                foreach (var album in collection.Items)
                {
                    yield return album;
                }
            }
        }

        private async Task<Collection> GetCollection(Dictionary<string, string> queryString)
        {
            var uri = "/api/collectionsync/1/collection";
            var query = queryString.Join("&", pair => pair.Key + "=" + pair.Value);
            var requestUri = uri + "?" + query;
            var accessToken = (await loginTask).AccessToken;

            using var message = new HttpRequestMessage(HttpMethod.Get, requestUri);
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await client.SendAsync(message);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Collection>(serializerOptions);
        }

        private async Task<LoginResponse> Login(string username, string password, string clientId, string clientSecret)
        {
            for (int i = 0; i < MaxLoginAttempts; ++i)
            {
                using var content = new FormUrlEncodedContent(GetLoginParameters(username, password, clientId, clientSecret));
                using var response = await SendRequest<LoginResponse>(content, "oauth_login"); 

                var status = (int)response.StatusCode;

                if (status == 418 || status == 451)
                {
                    continue;
                }

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<LoginResponse>(serializerOptions);
            }

            throw new Exception("Max number of login attempts exceeded.");
        }

        private async Task<HttpResponseMessage> SendRequest<T>(HttpContent content, string requestUri)
        {
            var requestBody = await content.ReadAsStringAsync();

            using var message = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = content,
                Version = new Version(2, 0)
            };

            SetHeaders(message.Headers, requestBody);

            var response = await client.SendAsync(message);
            OnHeadersReceived(response.Headers);

            return response;
        }

        private void SetHeaders(HttpRequestHeaders headers, string requestBody)
        {
            // X-Requested-With: XMLHttpRequest
            headers.Add("X-Requested-With", RequestedWith);

            var bandcampDm = Headers.CalculateDm(dm, requestBody);
            logger.LogInformation($"Sending {XBandcampDm} header {bandcampDm}");
            headers.Add(XBandcampDm, bandcampDm);

            if (pow != null)
            {
                var bandcampPow = Headers.CalculatePow(pow, requestBody);
                logger.LogInformation($"Sending {XBandcampPow} header {bandcampPow}");
                headers.Add(XBandcampPow, bandcampPow);
            }
        }

        private void OnHeadersReceived(HttpResponseHeaders headers)
        {
            dm = headers.GetValues(XBandcampDm).Single();
            logger.LogInformation($"Received {XBandcampDm} header {dm}");

            if (headers.TryGetValues(XBandcampPow, out var values))
            {
                pow = values.Single();
                logger.LogInformation($"Received {XBandcampPow} header {pow}");
            }
        }

        private static IEnumerable<KeyValuePair<string, string>> GetLoginParameters(
            string username,
            string password,
            string clientId,
            string clientSecret
        )
        {
            var dictionary = new Dictionary<string, string>();
            dictionary.Add("grant_type", "password");
            dictionary.Add("username", username);
            dictionary.Add("password", password);
            dictionary.Add("client_id", clientId);
            dictionary.Add("client_secret", clientSecret);

            return dictionary;
        }

        public void Dispose()
        {
            handler.Dispose();
            client.Dispose();
        }
    }
}