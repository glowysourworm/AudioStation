using System.IO;

using ATL;

using AudioStation.Core.Model.Interface;
using AudioStation.Core.Model.Vendor;
using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;
using AudioStation.Model;

using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.CoverArt;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.Extensions.Event;
using SimpleWpf.IocFramework.Application.Attribute;

using Query = MetaBrainz.MusicBrainz.Query;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(IMusicBrainzClient))]
    public class MusicBrainzClient : IMusicBrainzClient
    {
        // IAudioStationComponent
        //
        public event SimpleEventHandler<IAudioStationService, IAudioStationService.Status> StatusChangeEvent;

        private IAudioStationService.Status _status;

        [IocImportingConstructor]
        public MusicBrainzClient()
        {
        }

        /// <summary>
        /// Tries to look up information for the provided library entry
        /// </summary>
        public Task<IEnumerable<MusicBrainzRecording>> Query(string artist, string album, string trackName, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();

                    var searchResults = await query.FindRecordingsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));

                    return searchResults.Results
                                        .Where(result => result.Score >= minScore)
                                        .Select(result => ApplicationHelpers.Map<IRecording, MusicBrainzRecording>(result.Item))
                                        .ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return Enumerable.Empty<MusicBrainzRecording>();
            });
        }

        public Task<IEnumerable<MusicBrainzArtist>> QueryArtist(string artistName)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();

                    var searchResults = await query.FindArtistsAsync(string.Format("artist:{1}", artistName));

                    return searchResults.Results
                                        .Select(result => ApplicationHelpers.Map<IArtist, MusicBrainzArtist>(result.Item))
                                        .ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return Enumerable.Empty<MusicBrainzArtist>();
            });
        }

        public Task<MusicBrainzTrack?> GetTrack(string trackName, string albumName, string artistName, int searchScoreMin)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();

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

                            return ApplicationHelpers.Map<ITrack, MusicBrainzTrack>(track);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        public Task<MusicBrainzTrack?> GetTrack(Guid trackId, string trackName, string albumName, string artistName, int searchScoreMin)
        {
            return Task.Run(async () =>
            {

                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();

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

                            return ApplicationHelpers.Map<ITrack, MusicBrainzTrack>(track);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        public Task<MusicBrainzCombined?> GetCombinedData(string artistName, string albumName, string trackName)
        {
            return Task.Run(async () =>
            {

                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();

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
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        /// <summary>
        /// Gathers information for the combined view model - which should be for a single track. The track name is provided in case the
        /// Id on the tag is out of date.
        /// </summary>
        public Task<MusicBrainzCombined?> GetCombinedData(Guid releaseId, Guid artistId, Guid trackId, string trackName)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();
                    var artist = await query.LookupArtistAsync(artistId);
                    var release = await query.LookupReleaseAsync(releaseId, Include.Labels |
                                                                            Include.Media |
                                                                            Include.UrlRelationships |
                                                                            Include.Genres |
                                                                            Include.Tags |
                                                                            Include.Recordings);

                    var media = release.Media?.FirstOrDefault(x => x.Tracks?.Any(z => z.Id == trackId || z.Title == trackName) ?? false);

                    var coverArtClient = new CoverArt();
                    var frontArt = release.CoverArtArchive?.Front ?? false ? await coverArtClient.FetchFrontAsync(releaseId) : null;
                    var backArt = release.CoverArtArchive?.Back ?? false ? await coverArtClient.FetchBackAsync(releaseId) : null;

                    //var front = frontArt != null ? ConvertImage(frontArt, PictureInfo.PIC_TYPE.Front) : null;
                    //var bacK = backArt != null ? ConvertImage(backArt, PictureInfo.PIC_TYPE.Back) : null;

                    if (release == null || artist == null)
                    {
                        ApplicationHelpers.Log("Music Brainz Client failed for:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, null, trackName);
                        return null;
                    }

                    if (media == null)
                    {
                        ApplicationHelpers.Log("Music Brainz Client failed to retrieve media collection for:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, null, trackName);
                        return null;
                    }

                    // If the track id is out of date, try the track name
                    var track = media.Tracks?.FirstOrDefault(x => x.Id == trackId || x.Title == trackName);
                    var mediaIndex = release.Media?.IndexOf(media) ?? 0;

                    if (track == null)
                    {
                        ApplicationHelpers.Log("Music Brainz Client failed to retrieve track for:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, null, trackName);
                        return null;
                    }

                    var viewModel = new MusicBrainzCombined()
                    {
                        ArtistId = artistId,
                        Annotation = track.Recording?.Annotation ?? string.Empty,
                        ArtistCreditName = track.Recording?.ArtistCredit?.FirstOrDefault()?.Name ?? artist.Name ?? string.Empty,
                        Asin = release.Asin ?? string.Empty,
                        FrontCover = null,
                        BackCover = null,
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
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        public Task<MusicBrainzArtist?> GetArtistById(Guid musicBrainzArtistId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();
                    var include = Include.Releases |
                                  Include.Recordings |
                                  Include.Media |
                                  Include.Genres |
                                  Include.UrlRelationships |
                                  Include.LabelRelationships |
                                  Include.EventRelationships;

                    var result = await query.LookupArtistAsync(musicBrainzArtistId, include);

                    return ApplicationHelpers.Map<IArtist, MusicBrainzArtist>(result);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        public Task<MusicBrainzDisc?> GetDiscById(Guid musicBrainzDiscId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var include = Include.Artists |
                                  Include.Releases |
                                  Include.Recordings |
                                  Include.Media |
                                  Include.Genres |
                                  Include.UrlRelationships |
                                  Include.LabelRelationships |
                                  Include.EventRelationships;

                    var result = await query.LookupDiscIdAsync(musicBrainzDiscId.ToString(), null, include);

                    return ApplicationHelpers.Map<IDisc, MusicBrainzDisc>(result.Disc);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        public Task<IRelease?> GetReleaseById(Guid musicBrainzReleaseId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();

                    var include = Include.Artists |
                                  Include.Recordings |
                                  Include.Media |
                                  Include.Genres |
                                  Include.UrlRelationships |
                                  Include.LabelRelationships |
                                  Include.EventRelationships;

                    return await query.LookupReleaseAsync(musicBrainzReleaseId, include);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        public Task<IReleaseGroup?> GetReleaseGroupById(Guid musicBrainzReleaseGroupId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();

                    var include = Include.Artists |
                                  Include.Media |
                                  Include.Genres |
                                  Include.Releases |
                                  Include.UrlRelationships |
                                  Include.LabelRelationships |
                                  Include.EventRelationships;

                    return await query.LookupReleaseGroupAsync(musicBrainzReleaseGroupId, include);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        public Task<MusicBrainzRecording?> GetRecordingById(Guid musicBrainzRecordingId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();
                    var include = Include.Artists |
                                  Include.Genres |
                                  Include.Tags |
                                  Include.Releases |
                                  Include.Media;

                    var result = await query.LookupRecordingAsync(musicBrainzRecordingId, include);

                    return ApplicationHelpers.Map<IRecording, MusicBrainzRecording>(result);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        //public Task<IEnumerable<MusicBrainzPicture>> GetCoverArt(Guid musicBrainzReleaseId)
        //{
        //    return Task.Run(async () =>
        //    {
        //        try
        //        {
        //            var release = await GetReleaseById(musicBrainzReleaseId);

        //            var coverArtClient = new CoverArt();
        //            var coverArt = new List<CoverArtImage>();
        //            var result = new List<MusicBrainzPicture>();

        //            if (release.CoverArtArchive.Front)
        //            {
        //                var front = await coverArtClient.FetchFrontAsync(musicBrainzReleaseId);
        //                if (front != null)
        //                    result.Add(ConvertImage(front, PictureInfo.PIC_TYPE.Front));
        //            }

        //            if (release.CoverArtArchive.Back)
        //            {
        //                var back = await coverArtClient.FetchBackAsync(musicBrainzReleaseId);
        //                if (back != null)
        //                    coverArt.Add(back);
        //            }

        //            return result;
        //        }
        //        catch (Exception ex)
        //        {
        //            ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
        //        }

        //        return Enumerable.Empty<MusicBrainzPicture>();
        //    });
        //}

        public Task<MusicBrainzUrl?> GetUrlById(Guid musicBrainzUrlId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();
                    var include = Include.Artists |
                                  Include.Media |
                                  Include.Genres |
                                  Include.UrlRelationships |
                                  Include.LabelRelationships |
                                  Include.EventRelationships;

                    var result = await query.LookupUrlAsync(musicBrainzUrlId, include);

                    return ApplicationHelpers.Map<IUrl, MusicBrainzUrl>(result);
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                }

                return null;
            });
        }

        #region Entity Calls (query and insert into localDB)

        public Task<IEnumerable<IArtist>> QueryArtists(string artist, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await query.FindArtistsAsync(string.Format("artist:{0}", artist));
                    return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<IArtist>();
                }
            });
        }

        public Task<IEnumerable<IRecording>> QueryRecordings(string artist, string album, string trackName, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await query.FindRecordingsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));
                    return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<IRecording>();
                }
            });
        }

        public Task<IEnumerable<IRelease>> QueryReleases(string artist, string album, string trackName, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await query.FindReleasesAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));
                    return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<IRelease>();
                }
            });
        }

        public Task<IEnumerable<IDisc>> QueryDiscs(string artist, string album, string trackName, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await QueryMedia(artist, album, trackName, minScore);
                    return result.SelectMany(x => x.Discs ?? Enumerable.Empty<IDisc>()).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<IDisc>();
                }
            });
        }

        public Task<IEnumerable<IGenre>> GetAllGenres()
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await query.BrowseAllGenresAsync();
                    return result.Results.ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<IGenre>();
                }
            });
        }

        public Task<IEnumerable<ITag>> GetAllTags()
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await query.FindTagsAsync("*");
                    return result.Results.Select(x => x.Item).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<ITag>();
                }
            });
        }

        public Task<IEnumerable<ILabel>> QueryLabel(string artist, string album, string trackName, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await query.FindLabelsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));
                    return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<ILabel>();
                }
            });
        }

        public Task<IEnumerable<IMedium>> QueryMedia(string artist, string album, string trackName, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await QueryReleases(artist, album, trackName, minScore);
                    return result.SelectMany(x => x.Media ?? Enumerable.Empty<IMedium>()).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<IMedium>();
                }
            });
        }

        public Task<IEnumerable<IUrl>> GetRelatedUrls(string artist, string album, string trackName, int minScore)
        {
            return Task.Run(async () =>
            {
                try
                {
                    var query = new Query();
                    var result = await query.FindUrlsAsync(string.Format("title:{0} artist:{1} release:{2}", trackName, artist, album));
                    return result.Results.Where(x => x.Score >= minScore).Select(x => x.Item).ToList();
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message);
                    return Enumerable.Empty<IUrl>();
                }
            });
        }

        #endregion

        //private MusicBrainzPicture ConvertImage(CoverArtImage musicBrainzArt, PictureInfo.PIC_TYPE type)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        musicBrainzArt.Data.CopyTo(stream);

        //        var pictureInfo = PictureInfo.fromBinaryData(stream.GetBuffer(), type);

        //        return new MusicBrainzPicture(pictureInfo, false);
        //    }
        //}

        protected Task<bool> Authenticate()
        {
            // There is no application identification for MusicBrainz, so most of the normal 
            // authentication data is not required to establish a connection. We'll just run
            // a simple query to verify

            return Task.Run(() =>
            {
                try
                {
                    OnStatusChanged(IAudioStationService.Status.Working);

                    // Initialize MetaBrainz.MusicBrainz client
                    var query = new Query();
                    var searchResults = query.FindAllArtists("artist:Coldplay", 1);

                    OnStatusChanged(IAudioStationService.Status.Idle);

                    return true;
                }
                catch (Exception ex)
                {
                    ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageDbType.MusicBrainz, LogLevel.Error, ex, ex.Message);

                    OnStatusChanged(IAudioStationService.Status.Error);

                    return false;
                }
            });
        }

        #region (private) Release ID Lookup
        private Include CreateInclude(AudioStationTagServiceModel serviceModel)
        {
            // This should be broken down based on what is needed in the tag. Performance should vary depending on
            // how many fields, or what the lookup takes for field combinations.
            //
            return Include.Artists |
                   Include.Genres |
                   Include.Tags |
                   Include.Releases |
                   Include.Media;
        }
        private IAudioStationTag MapRecording(IRecording recording)
        {
            var release = recording.Releases?.FirstOrDefault(x => x.Date == recording.FirstReleaseDate);
            var media = release?.Media?.FirstOrDefault(x => x.Tracks.Any(z => z.Title == recording.Title));
            var track = media?.Tracks?.FirstOrDefault(x => x.Title == recording.Title);
            var artist = recording.ArtistCredit?.FirstOrDefault();
            var artistName = artist?.Name ?? artist?.Artist?.Name ?? string.Empty;

            return new AudioStationTag()
            {
                Album = release?.Title ?? string.Empty,
                Artist = artistName,
                Date = recording.FirstReleaseDate?.NearestDate ?? DateTime.MinValue,
                AlbumArtist = artistName,
                AlbumArtists = recording.ArtistCredit?.Select(x => x.Name ?? x.Artist?.Name ?? string.Empty)?.ToList() ?? new List<string>(),
                DiscNumber = (ushort)(media?.Position ?? 0),
                DiscTotal = (ushort)(release?.Media?.Count ?? 0),
                Duration = recording.Length ?? TimeSpan.Zero,
                Genre = release?.Genres?.FirstOrDefault()?.Name ?? string.Empty,
                Publisher = release?.LabelInfo?.FirstOrDefault()?.Label?.Name ?? string.Empty,
                TrackNumber = track?.Number ?? string.Empty,
                TrackTotal = (ushort)(media?.TrackCount ?? 0),
                Title = track?.Title ?? string.Empty,
                Track = (uint)(track?.Position ?? 0),
                Year = release?.Date?.Year ?? 0
            };
        }
        private async Task<PictureInfo?> LookupArtMusicBrainzId(Guid recordingId, bool frontOrBack)
        {
            var coverArtClient = new CoverArt();
            var query = new Query();
            var recording = await query.LookupRecordingAsync(recordingId, Include.Releases);
            var release = recording?.Releases?.FirstOrDefault(x => x.Date == recording.FirstReleaseDate);


            var hasArt = frontOrBack ? (release?.CoverArtArchive?.Front ?? false) : (release?.CoverArtArchive?.Back ?? false);

            if (!hasArt)
                return null;

            var art = frontOrBack ? await coverArtClient.FetchFrontAsync(release.Id) : await coverArtClient.FetchBackAsync(release.Id);

            if (art == null)
                return null;

            else
            {
                using (var streamReader = new BinaryReader(art.Data))
                {
                    var binaryData = streamReader.ReadBytes((int)art.Data.Length);
                    var pictureInfo = PictureInfo.fromBinaryData(binaryData, PictureInfo.PIC_TYPE.Front);

                    art.Dispose();

                    return pictureInfo;
                }
            }
        }
        private async Task<IRecording?> LookupTrack(string artist, string album, string title, Include include)
        {
            var query = new Query();

            var searchResults = await query.FindRecordingsAsync(string.Format("title:{0} artist:{1} release:{2}", title, artist, album));

            if (searchResults.Results.Count > 1)
                ApplicationHelpers.Log("Music Brainz artist/album/title search is returning more than one result with 100% score:  {0}/{1}/{2}", LogMessageServiceType.MusicBrainz, LogLevel.Warning, null, artist, album, title);

            return searchResults.Results
                                .Where(result => result.Score >= 100)
                                .FirstOrDefault()?.Item;
        }
        private async Task<IAudioStationTag?> LookupByArtistAlbumTitle(AudioStationTagServiceModel serviceModel)
        {
            var result = await LookupTrack(serviceModel.Artist, serviceModel.Album, serviceModel.Title, CreateInclude(serviceModel));

            if (result == null)
            {
                ApplicationHelpers.Log("Music Brainz artist/album/title search did not find any results:  {0}/{1}/{2}", LogMessageServiceType.MusicBrainz, LogLevel.Warning, null, serviceModel.Artist, serviceModel.Album, serviceModel.Title);
                return null;
            }

            else
                return MapRecording(result);
        }
        private async Task<IAudioStationTag?> LookupByMusicBrainzId(AudioStationTagServiceModel serviceModel)
        {
            var query = new Query();
            var coverArtClient = new CoverArt();

            var recording = await query.LookupRecordingAsync(serviceModel.MusicBrainzRecordingId, CreateInclude(serviceModel));

            if (recording == null)
            {
                ApplicationHelpers.Log("Music Brainz recording not found:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, null, serviceModel.MusicBrainzRecordingId);
                return null;
            }
            else
                return MapRecording(recording);
        }
        private async Task<IAudioStationTag?> Lookup(AudioStationTagServiceModel serviceModel)
        {
            switch (serviceModel.IdType)
            {
                case AudioStationTagServiceModel.AudioStationTagIdentity.ArtistAlbumTitle:
                    return await LookupByArtistAlbumTitle(serviceModel);
                case AudioStationTagServiceModel.AudioStationTagIdentity.MusicBrainzId:
                    return await LookupByMusicBrainzId(serviceModel);
                case AudioStationTagServiceModel.AudioStationTagIdentity.VendorId:
                    throw new Exception("Unhandled AudioStationTagIdentity type:  VendorId not used by IMusicBrainzClient service");
                default:
                    throw new Exception("Unhandled AudioStationTagIdentity type");
            }
        }
        private async Task<ITagSmall?> LookupSmall(AudioStationTagServiceModel serviceModel)
        {
            IAudioStationTag? result = null;

            switch (serviceModel.IdType)
            {
                case AudioStationTagServiceModel.AudioStationTagIdentity.ArtistAlbumTitle:
                    result = await LookupByArtistAlbumTitle(serviceModel);
                    break;
                case AudioStationTagServiceModel.AudioStationTagIdentity.MusicBrainzId:
                    result = await LookupByMusicBrainzId(serviceModel);
                    break;
                case AudioStationTagServiceModel.AudioStationTagIdentity.VendorId:
                    throw new Exception("Unhandled AudioStationTagIdentity type:  VendorId not used by IMusicBrainzClient service");
                default:
                    throw new Exception("Unhandled AudioStationTagIdentity type");
            }

            if (result != null)
                return TagMapper.MapTo(result, VendorNames.MusicBrainz, serviceModel.MusicBrainzRecordingId);
            else
                return null;
        }
        #endregion

        #region (public) IAudioStationTagService
        public Task<IAudioStationTag?> GetTagAsync(AudioStationTagServiceModel serviceModel)
        {
            return Task.Run(async () =>
            {
                return await Lookup(serviceModel);
            });
        }
        public Task<ITagSmall?> GetTagSmallAsync(AudioStationTagServiceModel serviceModel)
        {
            return Task.Run(async () =>
            {
                return await LookupSmall(serviceModel);
            });
        }
        public IAudioStationTag? GetTag(AudioStationTagServiceModel serviceModel)
        {
            return Lookup(serviceModel).Result;
        }
        public ITagSmall? GetTagSmall(AudioStationTagServiceModel serviceModel)
        {
            return LookupSmall(serviceModel).Result;
        }
        public Task<PictureInfo?> GetFrontArtAsync(AudioStationTagServiceModel serviceModel)
        {
            return LookupArtMusicBrainzId(serviceModel.MusicBrainzRecordingId, true);

        }
        public PictureInfo? GetFrontArt(AudioStationTagServiceModel serviceModel)
        {
            return LookupArtMusicBrainzId(serviceModel.MusicBrainzRecordingId, true).Result;
        }
        public Task<PictureInfo?> GetBackArtAsync(AudioStationTagServiceModel serviceModel)
        {
            return LookupArtMusicBrainzId(serviceModel.MusicBrainzRecordingId, false);
        }
        public PictureInfo? GetBackArt(AudioStationTagServiceModel serviceModel)
        {
            return LookupArtMusicBrainzId(serviceModel.MusicBrainzRecordingId, false).Result;
        }
        #endregion

        #region (public) IAudioStationComponent Methods
        public string GetName()
        {
            return "Music Brainz Client";
        }
        public string GetDisplayName()
        {
            return "Music Brainz Client";
        }
        public IAudioStationService.Status GetStatus()
        {
            return _status;
        }
        public async Task<IAudioStationService.Status> Initialize(Configuration configuration)
        {
            await Authenticate();

            return _status;
        }
        public Task<IAudioStationService.Status> ReInitialize(Configuration configuration)
        {
            return Initialize(configuration);
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
