using AudioDB;

using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Model.Vendor;

using SimpleWpf.IocFramework.Application.Attribute;

using Artist = AudioDB.Artist;

namespace AudioStation.Core.Component.Vendor
{
    [IocExport(typeof(IAudioDBClient))]
    public class AudioDBClient : IAudioDBClient
    {
        [IocImportingConstructor]
        public AudioDBClient()
        {

        }

        public Task<AudioDBArtist> SearchArtist(string artistName)
        {
            return Task.Run<AudioDBArtist>(() =>
            {
                var artist = AudioAPI.SearchArtist(artistName);
                long artistId = 0;

                if (artist != null && long.TryParse(artist.idArtist, out artistId))
                {
                    var albums = AudioAPI.SearchAlbumsByArtistId(artistId);
                    if (albums != null)
                    {
                        // Map Albums
                        var albumViewModels = albums.Select(album => MapAlbum(album));

                        // Map Artist
                        return MapArtist(artist, albumViewModels);
                    }
                }

                return null;
            });
        }

        private AudioDBArtist MapArtist(Artist artist, IEnumerable<AudioDBAlbum> albums)
        {
            int yearFormed, idArtist, idLabel;

            int.TryParse(artist.idLabel, out idLabel);
            int.TryParse(artist.idArtist, out idArtist);
            int.TryParse(artist.intFormedYear, out yearFormed);

            return new AudioDBArtist()
            {
                Albums = new List<AudioDBAlbum>(albums),
                ArtistBanner = artist.strArtistBanner ?? string.Empty,
                ArtistBio = artist.strBiographyEN ?? string.Empty,
                ArtistClearart = artist.strArtistClearart ?? string.Empty,
                ArtistCutout = artist.strArtistCutout ?? string.Empty,
                ArtistFanart = artist.strArtistFanart ?? string.Empty,
                ArtistFanart2 = artist.strArtistFanart2 ?? string.Empty,
                ArtistFanart3 = artist.strArtistFanart3 ?? string.Empty,
                ArtistFanart4 = artist.strArtistFanart4 ?? string.Empty,
                ArtistLogo = artist.strArtistLogo ?? string.Empty,
                ArtistName = artist.strArtist ?? string.Empty,
                ArtistThumb = artist.strArtistThumb ?? string.Empty,
                ArtistWideThumb = artist.strArtistWideThumb ?? string.Empty,
                Country = artist.strCountry ?? string.Empty,
                Facebook = artist.strFacebook ?? string.Empty,
                IdArtist = idArtist,
                IdLabel = idLabel,
                MusicBrainzId = artist.strMusicBrainzID ?? string.Empty,
                Website = artist.strWebsite ?? string.Empty,
                YearFormed = yearFormed
            };
        }

        private AudioDBAlbum MapAlbum(Album album)
        {
            int yearReleased, idAlbum, idArtist, idLabel;

            int.TryParse(album.idAlbum, out idAlbum);
            int.TryParse(album.idLabel, out idLabel);
            int.TryParse(album.idArtist, out idArtist);
            int.TryParse(album.intYearReleased, out yearReleased);

            return new AudioDBAlbum()
            {
                Album3DCase = album.strAlbum3DCase ?? string.Empty,
                Album3DFace = album.strAlbum3DFace ?? string.Empty,
                Album3DFlat = album.strAlbum3DFlat ?? string.Empty,
                Album3DThumb = album.strAlbum3DThumb ?? string.Empty,
                AlbumArt = album.strAlbum ?? string.Empty,
                AlbumBack = album.strAlbumThumbBack ?? string.Empty,
                AlbumName = album.strAlbum ?? string.Empty,
                AlbumThumb = album.strAlbumThumb ?? string.Empty,
                AlbumThumbHQ = album.strAlbumThumbHQ ?? string.Empty,
                Description = album.strDescriptionEN ?? string.Empty,
                IdAlbum = idAlbum,
                IdArtist = idArtist,
                IdLabel = idLabel,
                AllMusicId = album.strAllMusicID ?? string.Empty,
                AmazonId = album.strAmazonID ?? string.Empty,
                BBCReviewId = album.strBBCReviewID ?? string.Empty,
                DiscogsId = album.strDiscogsID ?? string.Empty,
                GeniusId = album.strGeniusID ?? string.Empty,
                ITunesId = album.strItunesID ?? string.Empty,
                LyricWikiId = album.strLyricWikiID ?? string.Empty,
                MusicMozId = album.strMusicMozID ?? string.Empty,
                RateYourMusicId = album.strRateYourMusicID ?? string.Empty,
                WikidataId = album.strWikidataID ?? string.Empty,
                WikipediaId = album.strWikipediaID ?? string.Empty,
                MusicBrainzArtistId = album.strMusicBrainzArtistID ?? string.Empty,
                MusicBrainzId = album.strMusicBrainzID ?? string.Empty,
                YearReleased = yearReleased
            };
        }
    }
}
