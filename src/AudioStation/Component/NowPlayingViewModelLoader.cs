using AudioStation.Component.Interface;
using AudioStation.Component.Model;
using AudioStation.ViewModels.LibraryViewModels;
using AudioStation.ViewModels.PlaylistViewModels;

using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Component
{
    [IocExport(typeof(INowPlayingViewModelLoader))]
    public class NowPlayingViewModelLoader : INowPlayingViewModelLoader
    {
        private readonly ILastFmClient _lastFmClient;
        private readonly ISpotifyClient _spotifyClient;
        private readonly IMusicBrainzClient _musicBrainzClient;

        [IocImportingConstructor]
        public NowPlayingViewModelLoader(ILastFmClient lastFmClient, ISpotifyClient spotifyClient, IMusicBrainzClient musicBrainzClient)
        {
            _lastFmClient = lastFmClient;
            _spotifyClient = spotifyClient;
            _musicBrainzClient = musicBrainzClient;
        }

        public async Task<NowPlayingData> LoadPlaylist(ArtistViewModel artist, AlbumViewModel album, LibraryEntryViewModel startTrack)
        {
            var lastFmResponse = await _lastFmClient.GetNowPlayingInfo(artist.Artist, album.Album);
            var spotifyResponse = await _spotifyClient.CreateNowPlaying(artist.Artist, album.Album);
            var musicBrainzResponse = await _musicBrainzClient.QueryArtist(artist.Artist);

            var playlistEntries = new List<PlaylistEntryViewModel>();

            foreach (var track in album.Tracks)
            {
                playlistEntries.Add(new PlaylistEntryViewModel(artist, album, track));
            }

            return new NowPlayingData()
            {
                ArtistArticle = lastFmResponse?.BioContent ?? string.Empty,
                ArtistSummary = lastFmResponse?.BioSummary ?? string.Empty,
                BestImage = spotifyResponse?.CombinedImages?.FirstOrDefault() ?? lastFmResponse?.AlbumImage ?? lastFmResponse?.ArtistMainImage ?? string.Empty,
                ExternalLinks = musicBrainzResponse?.FirstOrDefault()?
                                                    .Relationships?
                                                    .Where(x => x.Url != null)?
                                                    .Select(x => x.Url?.ToString() ?? string.Empty) ?? Enumerable.Empty<string>(),
                Entries = playlistEntries,
                NowPlaying = playlistEntries.First(x => x.Track.Id == startTrack.Id)
            };
        }
    }
}
