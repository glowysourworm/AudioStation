using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Threading;

using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.SpotifyViewModel;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.ObservableCollection;
using SimpleWpf.IocFramework.Application.Attribute;

using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;

using ISpotifyClient = AudioStation.Component.Vendor.Interface.ISpotifyClient;

namespace AudioStation.Component.Vendor
{
    [IocExport(typeof(ISpotifyClient))]
    public class SpotifyClient : ISpotifyClient
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IOutputController _outputController;

        private const string SPOTIFY_WEB_SEARCH = "https://api.spotify.com/v1/search";

        [IocImportingConstructor]
        public SpotifyClient(IConfigurationManager configurationManager, IOutputController outputController)
        {
            _configurationManager = configurationManager;
            _outputController = outputController;
        }

        public Task<SpotifyNowPlayingViewModel?> CreateNowPlaying(string artistName, string albumName)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var configuration = _configurationManager.GetConfiguration();
                    var authenticator = new ClientCredentialsAuthenticator(configuration.SpotifyClientId, configuration.SpotifyClientSecret);

                    var connector = new APIConnector(new Uri(SPOTIFY_WEB_SEARCH), authenticator);
                    var client = new SearchClient(connector);

                    var artistResponse = await client.Item(new SearchRequest(SearchRequest.Types.Artist, artistName));
                    var albumResponse = await client.Item(new SearchRequest(SearchRequest.Types.Album, albumName));

                    var artist = artistResponse.Artists?.Items?.FirstOrDefault(x => x.Name == artistName);

                    if (artist != null)
                    {
                        var album = albumResponse?.Albums?.Items?.FirstOrDefault(x => x.Artists.Any(z => z.Id == artist.Id) &&
                                                                                      x.Name == albumName);
                        if (album != null)
                        {
                            var result = new SpotifyNowPlayingViewModel()
                            {
                                ArtistUrl = artist.Uri ?? string.Empty,
                                ArtistImages = new ObservableCollection<string>(new string[] { artist.Images.MaxBy(x => { return x.Width * x.Height; })?.Url ?? string.Empty }),
                                ArtistExternalUrls = new ObservableCollection<string>(artist.ExternalUrls.Values),
                                AlbumUrl = album.Uri ?? string.Empty,
                                AlbumImages = new ObservableCollection<string>(new string[] { album.Images.MaxBy(x => { return x.Width * x.Height; })?.Url ?? string.Empty }),
                                AlbumExtenralUrls = new ObservableCollection<string>(album.ExternalUrls.Values)
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
                    ApplicationHelpers.Log("Error connecting to Spotify API:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
                    return null;
                }
            });
        }
    }
}
