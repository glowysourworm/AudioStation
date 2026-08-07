using AudioStation.Core.Component.Interface;
using AudioStation.Core.Controller.Interface;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;

using ISpotifyClient = AudioStation.Core.Service.Vendor.Interface.ISpotifyClient;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(ISpotifyClient))]
    public class SpotifyClient : ISpotifyClient
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IOutputController _outputController;

        //private const string SPOTIFY_WEB_SEARCH = "https://api.spotify.com/v1/search";
        private const string SPOTIFY_WEB_BASE = "https://api.spotify.com";

        // Spotify:  Primary client http connection
        private SpotifyAPI.Web.SpotifyClient? _client;

        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        private IAudioStationService.Status _status;

        [IocImportingConstructor]
        public SpotifyClient(IConfigurationManager configurationManager, IOutputController outputController)
        {
            _configurationManager = configurationManager;
            _outputController = outputController;
        }

        public Task<SpotifyNowPlaying?> CreateNowPlaying(string artistName, string albumName)
        {
            if (_status != IAudioStationService.Status.Idle)
                return null;

            if (_client == null)
                return null;

            return Task.Run(async () =>
            {
                try
                {
                    var artistResponse = await _client.Search.Item(new SearchRequest(SearchRequest.Types.Artist, artistName));
                    var albumResponse = await _client.Search.Item(new SearchRequest(SearchRequest.Types.Album, albumName));

                    var artist = artistResponse.Artists?.Items?.FirstOrDefault(x => x.Name == artistName);

                    if (artist != null)
                    {
                        var album = albumResponse?.Albums?.Items?.FirstOrDefault(x => x.Artists.Any(z => z.Id == artist.Id) &&
                                                                                      x.Name == albumName);
                        if (album != null)
                        {
                            var result = new SpotifyNowPlaying()
                            {
                                ArtistUrl = artist.Uri ?? string.Empty,
                                ArtistImages = new List<string>(new string[] { artist.Images.MaxBy(x => { return x.Width * x.Height; })?.Url ?? string.Empty }),
                                ArtistExternalUrls = new List<string>(artist.ExternalUrls.Values),
                                AlbumUrl = album.Uri ?? string.Empty,
                                AlbumImages = new List<string>(new string[] { album.Images.MaxBy(x => { return x.Width * x.Height; })?.Url ?? string.Empty }),
                                AlbumExtenralUrls = new List<string>(album.ExternalUrls.Values)
                            };

                            result.CombinedImages.AddRange(result.ArtistImages.Where(x => !string.IsNullOrEmpty(x)));
                            result.CombinedImages.AddRange(result.AlbumImages.Where(x => !string.IsNullOrEmpty(x)));
                            return result;
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error connecting to Spotify API:  {0}", LogMessageServiceType.Spotify, LogLevel.Error, ex, ex.Message);
                    return null;
                }
            });
        }

        protected Task<SpotifyAPI.Web.SpotifyClient?> Authenticate()
        {
            return Task.Run(async () =>
            {
                try
                {
                    var configuration = _configurationManager.GetConfiguration();
                    var authenticator = new ClientCredentialsAuthenticator(configuration.SpotifyClientId, configuration.SpotifyClientSecret);

                    // Not sure why I need to supply some of these components.. 
                    var clientConfiguration = new SpotifyClientConfig(new Uri(SPOTIFY_WEB_BASE),
                                                                      authenticator, new NewtonsoftJSONSerializer(),
                                                                      new SpotifyAPI.Web.Http.NetHttpClient(), null, null, null);

                    //var connector = new APIConnector(new Uri(SPOTIFY_WEB_BASE), authenticator);
                    return new SpotifyAPI.Web.SpotifyClient(clientConfiguration);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Error connecting to Spotify API:  {0}", LogMessageServiceType.Spotify, LogLevel.Error, ex, ex.Message);
                    return null;
                }
            });
        }

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "Spotify Client";
        }
        public string GetDisplayName()
        {
            return "Spotify Client";
        }
        public IAudioStationService.Status GetStatus()
        {
            return _status;
        }
        public async Task<IAudioStationService.Status> Initialize()
        {
            var configuration = _configurationManager.GetConfiguration();

            if (string.IsNullOrWhiteSpace(configuration.SpotifyClientId))
                OnStatusChanged(IAudioStationService.Status.Disabled);

            if (string.IsNullOrWhiteSpace(configuration.SpotifyClientSecret))
                OnStatusChanged(IAudioStationService.Status.Disabled);

            _client = await Authenticate();

            // -> Error
            if (_client == null)
                OnStatusChanged(IAudioStationService.Status.Error);

            // -> Idle
            else
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
        #endregion
    }
}
