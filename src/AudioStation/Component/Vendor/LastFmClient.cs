using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;

using AudioStation.Component.Vendor.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.LastFmViewModel;

using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Api.Enums;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component.Vendor
{
    [IocExport(typeof(ILastFmClient))]
    public class LastFmClient : ILastFmClient
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IOutputController _outputController;

        [IocImportingConstructor]
        public LastFmClient(IConfigurationManager configurationManager, IOutputController outputController)
        {
            _outputController = outputController;
            _configurationManager = configurationManager;
        }

        public async Task<LastFmNowPlayingViewModel> GetNowPlayingInfo(string artist, string album)
        {
            try
            {
                var configuration = _configurationManager.GetConfiguration();

                // Last FM API
                var client = new LastfmClient(configuration.LastFmAPIKey, configuration.LastFmAPISecret);

                // Album / Artist Detail
                var albumResponse = await client.Album.GetInfoAsync(artist, album, false);
                var artistResponse = await client.Artist.GetInfoAsync(artist);

                // Status OK -> Create bitmap image from the url
                if (albumResponse.Status == LastResponseStatus.Successful &&
                    artistResponse.Status == LastResponseStatus.Successful)
                {
                    return new LastFmNowPlayingViewModel()
                    {
                        AlbumImage = albumResponse.Content.Images?.Largest?.AbsoluteUri ?? string.Empty,
                        AlbumUrl = albumResponse.Content.Url?.AbsoluteUri ?? string.Empty,
                        ArtistMainImage = artistResponse.Content.MainImage?.Largest?.AbsoluteUri ?? string.Empty,
                        ArtistUrl = artistResponse.Content.Url?.AbsoluteUri ?? string.Empty,
                        ArtistYearFormed = artistResponse.Content.Bio?.YearFormed ?? 0,
                        BioContent = artistResponse.Content.Bio?.Content ?? string.Empty,
                        BioSummary = artistResponse.Content.Bio?.Summary ?? string.Empty,
                        Tracks = new ObservableCollection<LastFmTrackViewModel>(albumResponse.Content.Tracks.Select(track =>
                        {
                            return new LastFmTrackViewModel()
                            {
                                ArtistImage = track.ArtistImages?.Largest?.AbsoluteUri ?? string.Empty,
                                ArtistUrl = track.ArtistUrl?.AbsoluteUri ?? string.Empty,
                                Image = track.Images?.Largest?.AbsoluteUri ?? string.Empty,
                                Url = track.Url?.AbsoluteUri ?? string.Empty
                            };
                        }))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error contacting LastFm:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);

                return null;
            }
        }
    }
}
