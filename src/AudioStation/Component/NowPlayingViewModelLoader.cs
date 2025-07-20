using AudioStation.Component.Interface;
using AudioStation.Component.Model;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.PlaylistViewModels;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component
{
    [IocExport(typeof(INowPlayingViewModelLoader))]
    public class NowPlayingViewModelLoader : INowPlayingViewModelLoader
    {
        private readonly ILastFmClient _lastFmClient;
        private readonly ISpotifyClient _spotifyClient;
        private readonly IMusicBrainzClient _musicBrainzClient;
        private readonly IFanartClient _fanartClient;
        private readonly IWikipediaClient _wikipediaClient;
        private readonly IOutputController _outputController;

        [IocImportingConstructor]
        public NowPlayingViewModelLoader(ILastFmClient lastFmClient,
                                         ISpotifyClient spotifyClient,
                                         IMusicBrainzClient musicBrainzClient,
                                         IFanartClient fanartClient,
                                         IWikipediaClient wikipediaClient,
                                         IOutputController outputController)
        {
            _lastFmClient = lastFmClient;
            _spotifyClient = spotifyClient;
            _musicBrainzClient = musicBrainzClient;
            _fanartClient = fanartClient;
            _wikipediaClient = wikipediaClient;
            _outputController = outputController;
        }

        public async Task<NowPlayingData> LoadPlaylist(ArtistViewModel artist, AlbumViewModel album, LibraryEntryViewModel startTrack)
        {
            try
            {
                var wikipediaResponse = await _wikipediaClient.GetExcerpt(artist.Artist);
                var lastFmResponse = await _lastFmClient.GetNowPlayingInfo(artist.Artist, album.Album);
                var spotifyResponse = await _spotifyClient.CreateNowPlaying(artist.Artist, album.Album);
                var musicBrainzResponse = await _musicBrainzClient.QueryArtist(artist.Artist);

                var musicBrainzArtist = musicBrainzResponse?.FirstOrDefault();

                var fanartBackgrounds = musicBrainzArtist != null ? await _fanartClient.GetArtistBackgrounds(musicBrainzArtist.Id.ToString()) : null;
                var fanartArtistThumbs = musicBrainzArtist != null ? await _fanartClient.GetArtistImages(musicBrainzArtist.Id.ToString()) : null;

                var playlistEntries = new List<PlaylistEntryViewModel>();

                foreach (var track in album.Tracks)
                {
                    playlistEntries.Add(new PlaylistEntryViewModel(artist, album, track));
                }

                return new NowPlayingData()
                {
                    ArtistArticle = wikipediaResponse?.ExtractBody ?? lastFmResponse?.BioContent ?? string.Empty,
                    ArtistSummary = wikipediaResponse?.ExtractSummary ?? lastFmResponse?.BioSummary ?? string.Empty,
                    BestImage = spotifyResponse?.CombinedImages?.FirstOrDefault() ?? lastFmResponse?.AlbumImage ?? lastFmResponse?.ArtistMainImage ?? string.Empty,
                    ArtistImages = fanartArtistThumbs ?? Enumerable.Empty<string>(),
                    BackgroundImages = fanartBackgrounds ?? Enumerable.Empty<string>(),
                    ExternalLinks = musicBrainzResponse?.FirstOrDefault()?
                                                        .Relationships?
                                                        .Where(x => x.Url != null)?
                                                        .Select(x => x.Url?.ToString() ?? string.Empty) ?? Enumerable.Empty<string>(),
                    Entries = playlistEntries,
                    NowPlaying = playlistEntries.First(x => x.Track.Id == startTrack.Id)
                };
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Error gathering external resources:  {0}", LogMessageType.General, LogLevel.Error, ex, ex.Message);
                return null;
            }
        }
    }
}
