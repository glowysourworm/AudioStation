using AudioStation.Core.Component.Interface;
using AudioStation.Core.Component.Vendor.Interface;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Utility;
using AudioStation.Model;

using AutoMapper;

using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.CoverArt;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;

namespace AudioStation.Core.Component.Vendor
{
    [IocExport(typeof(IMusicBrainzClient))]
    public class MusicBrainzClient : IMusicBrainzClient
    {
        [IocImportingConstructor]
        public MusicBrainzClient()
        {
        }

        /// <summary>
        /// Tries to look up information for the provided library entry
        /// </summary>
        public async Task<IEnumerable<MusicBrainzRecording>> Query(string artist, string album, string trackName, int minScore)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IRecording, MusicBrainzRecording>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var searchResults = await query.FindRecordingsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));

                return searchResults.Results
                                    .Where(result => result.Score >= minScore)
                                    .Select(result => mapper.Map<MusicBrainzRecording>(result.Item))
                                    .ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<IEnumerable<MusicBrainzArtist>> QueryArtist(string artistName)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IArtist, MusicBrainzArtist>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var searchResults = await query.FindArtistsAsync(string.Format("artist:{1}", artistName));

                return searchResults.Results
                                    .Select(result => mapper.Map<MusicBrainzArtist>(result.Item))
                                    .ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzTrack> GetTrack(string trackName, string albumName, string artistName, int searchScoreMin)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<ITrack, MusicBrainzTrack>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var searchResults = await query.FindReleasesAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artistName, albumName));

                var result = searchResults.Results
                                          .Where(x => x.Score >= searchScoreMin)
                                          .OrderByDescending(x => x.Score)
                                          .FirstOrDefault(x => x.Item.Media?.Any(z => z.Tracks?.Any(w => w.Title == trackName) ?? false) ?? false);

                if (result != null)
                {
                    var albumId = result.Item.Id;
                    
                    // Get media based on track title
                    var media = result.Item
                                      .Media?
                                      .FirstOrDefault(x => x.Tracks?.Any(track => track.Title == trackName) ?? false);

                    if (media != null)
                    {
                        var track = media.Tracks?.First(x => x.Title == trackName);

                        return mapper.Map<MusicBrainzTrack>(track);
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzTrack> GetTrack(Guid trackId, string trackName, string albumName, string artistName, int searchScoreMin)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<ITrack, MusicBrainzTrack>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var searchResults = await query.FindReleasesAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artistName, albumName));

                var result = searchResults.Results
                                          .Where(x => x.Score >= searchScoreMin)
                                          .OrderByDescending(x => x.Score)
                                          .FirstOrDefault(x => x.Item.Media?.Any(z => z.Tracks?.Any(w => w.Id.ToString() == trackId.ToString()) ?? false) ?? false);

                if (result != null)
                {
                    var albumId = result.Item.Id;

                    // Get media based on track ID
                    var media = result.Item
                                      .Media?
                                      .FirstOrDefault(x => x.Tracks?.Any(track => track.Id.ToString() == trackId.ToString()) ?? false);

                    if (media != null)
                    {
                        var track = media.Tracks?.First(x => x.Id.ToString() == trackId.ToString());

                        return mapper.Map<MusicBrainzTrack>(track);
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzCombined> GetCombinedData(string artistName, string albumName, string trackName)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IRecording, MusicBrainzRecording>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var searchResults = await query.FindArtistsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artistName, albumName));

                var result = searchResults.Results.FirstOrDefault()?.Item;

                if (result != null)
                {
                    var artistId = result.Id;
                    var release = result.Releases?.FirstOrDefault(x => x.Title == albumName);

                    if (release != null)
                    {
                        var albumId = release.Id;
                        var releaseViewModel = await GetReleaseById(release.Id);

                        var media = releaseViewModel.Media?.FirstOrDefault(x => x.Tracks.Any(track => track.Title == trackName));

                        if (media != null)
                        {
                            var track = media.Tracks.First(x => x.Title == trackName);

                            return await GetCombinedData(release.Id, artistId, track.Id, trackName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gathers information for the combined view model - which should be for a single track. The track name is provided in case the
        /// Id on the tag is out of date.
        /// </summary>
        public async Task<MusicBrainzCombined> GetCombinedData(Guid releaseId, Guid artistId, Guid trackId, string trackName)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IArtist, MusicBrainzArtist>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var artist = await query.LookupArtistAsync(artistId);
                var release = await query.LookupReleaseAsync(releaseId, Include.Labels |
                                                                        Include.Media |
                                                                        Include.UrlRelationships |
                                                                        Include.Genres |
                                                                        Include.Tags |
                                                                        Include.Recordings);
                var media = release.Media?.FirstOrDefault(x => x.Tracks?.Any(z => z.Id == trackId || z.Title == trackName) ?? false);

                var coverArtClient = new CoverArt();
                var frontArt = release.CoverArtArchive?.Front ?? false ? coverArtClient.FetchFront(releaseId) : null;
                var backArt = release.CoverArtArchive?.Back ?? false ? coverArtClient.FetchBack(releaseId) : null;

                var front = frontArt != null ? new Image(frontArt.Data, frontArt.ContentType ?? string.Empty) : null;
                var bacK = backArt != null ? new Image(backArt.Data, backArt.ContentType ?? string.Empty) : null;

                if (release == null || artist == null)
                {
                    ApplicationHelpers.Log("Music Brainz Client failed for:  {0}", LogMessageType.Vendor, LogLevel.Error, trackName);
                    return null;
                }

                if (media == null)
                {
                    ApplicationHelpers.Log("Music Brainz Client failed to retrieve media collection for:  {0}", LogMessageType.Vendor, LogLevel.Error, trackName);
                    return null;
                }

                // If the track id is out of date, try the track name
                var track = media.Tracks?.FirstOrDefault(x => x.Id == trackId || x.Title == trackName);
                var mediaIndex = release.Media?.IndexOf(media) ?? 0;

                if (track == null)
                {
                    ApplicationHelpers.Log("Music Brainz Client failed to retrieve track for:  {0}", LogMessageType.Vendor, LogLevel.Error, trackName);
                    return null;
                }

                var viewModel = new MusicBrainzCombined()
                {
                    ArtistId = artistId,
                    Annotation = track.Recording?.Annotation ?? string.Empty,
                    ArtistCreditName = track.Recording?.ArtistCredit?.FirstOrDefault()?.Name ?? artist.Name ?? string.Empty,
                    Asin = release.Asin ?? string.Empty,
                    FrontCover = front,
                    BackCover = bacK,
                    AssociatedUrls = release.Relationships?
                                            .Where(x => x.TargetType == EntityType.Url)?
                                            .Select(x => x.Url?.Resource?.AbsoluteUri ?? string.Empty)?
                                            .ToList() ?? Enumerable.Empty<string>(),

                    Barcode = release.Barcode ?? string.Empty,
                    Disambiguation = track.Recording?.Disambiguation ?? string.Empty,
                    Genres = track.Recording?.Genres?.Select(x => x.Name ?? string.Empty)?.ToList() ?? Enumerable.Empty<string>(),
                    LabelCatalogNumber = release.LabelInfo?.FirstOrDefault()?.CatalogNumber ?? string.Empty,
                    LabelCode = release.LabelInfo?.FirstOrDefault()?.Label?.LabelCode ?? 0,
                    LabelCountry = release.LabelInfo?.FirstOrDefault()?.Label?.Country ?? string.Empty,
                    LabelIpis = release.LabelInfo?.FirstOrDefault()?.Label?.Ipis ?? Enumerable.Empty<string>(),
                    LabelName = release.LabelInfo?.FirstOrDefault()?.Label?.Name ?? string.Empty,
                    MediumDiscCount = media.Discs?.Count ?? 0,
                    MediumFormat = media.Format ?? string.Empty,
                    MediumTitle = media.Title ?? string.Empty,
                    MediumDiscPosition = mediaIndex + 1,
                    MediumTrackCount = media.TrackCount,
                    MediumTrackOffset = media.TrackOffset ?? 0,
                    Packaging = release.Packaging ?? string.Empty,
                    Quality = release.Quality ?? string.Empty,
                    ReleaseCountry = release.Country ?? string.Empty,
                    ReleaseDate = release.Date?.NearestDate ?? DateTime.MinValue,
                    ReleaseId = release.Id,
                    ReleaseStatus = release.Status ?? string.Empty,
                    ReleaseTitle = release.Title ?? string.Empty,
                    Tags = track.Recording?.Tags?.Select(x => x.Name)?.ToList() ?? Enumerable.Empty<string>(),
                    Title = track.Title ?? string.Empty,
                    Track = track,
                    TrackId = track.Id,
                    UserGenres = track.Recording?.UserGenres?.Select(x => x.Name ?? string.Empty)?.ToList() ?? Enumerable.Empty<string>(),
                    UserTags = track.Recording?.UserTags?.Select(x => x.Name)?.ToList() ?? Enumerable.Empty<string>(),
                };

                frontArt?.Dispose();

                backArt?.Dispose();

                frontArt = null;
                backArt = null;

                return viewModel;

            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzArtist> GetArtistById(Guid musicBrainzArtistId)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IArtist, MusicBrainzArtist>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Releases |
                              Include.Recordings |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupArtistAsync(musicBrainzArtistId, include);

                return mapper.Map<MusicBrainzArtist>(result);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzDisc> GetDiscById(Guid musicBrainzDiscId)
        {
            try
            {
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IArtist, MusicBrainzDisc>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Artists |
                              Include.Releases |
                              Include.Recordings |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupDiscIdAsync(musicBrainzDiscId.ToString(), null, include);

                return mapper.Map<MusicBrainzDisc>(result);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzRelease> GetReleaseById(Guid musicBrainzReleaseId)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IRelease, MusicBrainzRelease>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Artists |
                              Include.Recordings |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupReleaseAsync(musicBrainzReleaseId, include);

                return mapper.Map<MusicBrainzRelease>(result);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzRecording> GetRecordingById(Guid musicBrainzRecordingId)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IRecording, MusicBrainzRecording>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Artists |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupRecordingAsync(musicBrainzRecordingId, include);

                return mapper.Map<MusicBrainzRecording>(result);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzUrl> GetUrlById(Guid musicBrainzUrlId)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IUrl, MusicBrainzUrl>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Artists |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupUrlAsync(musicBrainzUrlId, include);

                return mapper.Map<MusicBrainzUrl>(result);
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
            }

            return null;
        }

        #region Entity Calls (query and insert into localDB)

        public async Task<IEnumerable<IArtist>> QueryArtists(string artist, int minScore)
        {
            try
            {
                var query = new Query();
                var result = await query.FindArtistsAsync(string.Format("artist:{0}", artist));
                return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public async Task<IEnumerable<IRecording>> QueryRecordings(string artist, string album, string trackName, int minScore)
        {
            try
            {
                var query = new Query();
                var result = await query.FindRecordingsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));
                return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public async Task<IEnumerable<IRelease>> QueryReleases(string artist, string album, string trackName, int minScore)
        {
            try
            {
                var query = new Query();
                var result = await query.FindReleasesAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));
                return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public async Task<IEnumerable<IDisc>> QueryDiscs(string artist, string album, string trackName, int minScore)
        {
            try
            {
                var query = new Query();
                var result = await QueryMedia(artist, album, trackName, minScore);
                return result.SelectMany(x => x.Discs ?? Enumerable.Empty<IDisc>()).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public async Task<IEnumerable<IGenre>> GetAllGenres()
        {
            try
            {
                var query = new Query();
                var result = await query.BrowseAllGenresAsync();
                return result.Results.ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public async Task<IEnumerable<ITag>> GetAllTags()
        {
            try
            {
                var query = new Query();
                var result = await query.FindTagsAsync("*");
                return result.Results.Select(x => x.Item).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public async Task<IEnumerable<ILabel>> QueryLabel(string artist, string album, string trackName, int minScore)
        {
            try
            {
                var query = new Query();
                var result = await query.FindLabelsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));
                return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public async Task<IEnumerable<IMedium>> QueryMedia(string artist, string album, string trackName, int minScore)
        {
            try
            {
                var query = new Query();
                var result = await QueryReleases(artist, album, trackName, minScore);
                return result.SelectMany(x => x.Media ?? Enumerable.Empty<IMedium>()).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        public async Task<IEnumerable<IUrl>> GetRelatedUrls(string artist, string album, string trackName, int minScore)
        {
            try
            {
                var query = new Query();
                var result = await query.FindUrlsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));
                return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageType.Vendor, LogLevel.Error, ex.Message);
                throw ex;
            }
        }

        #endregion
    }
}
