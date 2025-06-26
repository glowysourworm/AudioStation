using AudioStation.Component.Interface;
using AudioStation.Core.Component.Interface;
using AudioStation.Core.Model;
using AudioStation.Model;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

using AutoMapper;

using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.CoverArt;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.Extensions.Logging;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.Extensions.Collection;
using System.IO;
using AudioStation.ViewModels;

namespace AudioStation.Component
{
    [IocExport(typeof(IMusicBrainzClient))]
    public class MusicBrainzClient : IMusicBrainzClient
    {
        readonly IOutputController _outputController;

        [IocImportingConstructor]
        public MusicBrainzClient(IOutputController outputController)
        {
            _outputController = outputController;
        }

        /// <summary>
        /// Tries to look up information for the provided library entry
        /// </summary>
        public async Task<IEnumerable<MusicBrainzRecordingViewModel>> Query(LibraryEntry entry)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IRecording, MusicBrainzRecordingViewModel>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var searchResults = await query.FindRecordingsAsync(string.Format("title:{0} artist:{1} release:{2}", entry.Title, entry.PrimaryArtist, entry.Album));

                return searchResults.Results
                                    .Select(result => mapper.Map<MusicBrainzRecordingViewModel>(result.Item))
                                    .ToList();
            }
            catch (Exception ex)
            {
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<IEnumerable<MusicBrainzArtistViewModel>> QueryArtist(string artistName)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IArtist, MusicBrainzArtistViewModel>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var searchResults = await query.FindArtistsAsync(string.Format("artist:{1}", artistName));

                return searchResults.Results
                                    .Select(result => mapper.Map<MusicBrainzArtistViewModel>(result.Item))
                                    .ToList();
            }
            catch (Exception ex)
            {
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzCombinedViewModel> GetCombinedData(string artistName, string albumName, string trackName)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IRecording, MusicBrainzRecordingViewModel>(MemberList.Destination));
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
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        /// <summary>
        /// Gathers information for the combined view model - which should be for a single track. The track name is provided in case the
        /// Id on the tag is out of date.
        /// </summary>
        public async Task<MusicBrainzCombinedViewModel> GetCombinedData(Guid releaseId, Guid artistId, Guid trackId, string trackName)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IArtist, MusicBrainzArtistViewModel>(MemberList.Destination));
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

                var frontViewModel = frontArt != null ? new ImageViewModel(frontArt.Data, frontArt.ContentType ?? string.Empty) : null;
                var backViewModel = backArt != null ? new ImageViewModel(backArt.Data, backArt.ContentType ?? string.Empty) : null;

                if (release == null || artist == null)
                {
                    RaiseLog("Music Brainz Client failed for:  {0}", LogMessageType.General, LogLevel.Error, trackName);
                    return null;
                }

                if (media == null)
                {
                    RaiseLog("Music Brainz Client failed to retrieve media collection for:  {0}", LogMessageType.General, LogLevel.Error, trackName);
                    return null;
                }
                    
                // If the track id is out of date, try the track name
                var track = media.Tracks?.FirstOrDefault(x => x.Id == trackId || x.Title == trackName);
                var mediaIndex = release.Media?.IndexOf(media) ?? 0;

                if (track == null)
                {
                    RaiseLog("Music Brainz Client failed to retrieve track for:  {0}", LogMessageType.General, LogLevel.Error, trackName);
                    return null;
                }

                var viewModel = new MusicBrainzCombinedViewModel()
                {
                    ArtistId = artistId,
                    Annotation = track.Recording?.Annotation ?? string.Empty,
                    ArtistCreditName = track.Recording?.ArtistCredit?.FirstOrDefault()?.Name ?? artist.Name ?? string.Empty,
                    Asin = release.Asin ?? string.Empty,
                    FrontCover = frontViewModel,
                    BackCover = backViewModel,
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

                if (frontArt != null)
                    frontArt.Dispose();

                if (backArt != null)
                    backArt.Dispose();

                frontArt = null;
                backArt = null;

                return viewModel;
                
            }
            catch (Exception ex)
            {
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzArtistViewModel> GetArtistById(Guid musicBrainzArtistId)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IArtist, MusicBrainzArtistViewModel>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Releases |
                              Include.Recordings |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupArtistAsync(musicBrainzArtistId, include);

                return mapper.Map<MusicBrainzArtistViewModel>(result);
            }
            catch (Exception ex)
            {
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzDiscViewModel> GetDiscById(Guid musicBrainzDiscId)
        {
            try
            {
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IArtist, MusicBrainzDiscViewModel>(MemberList.Destination));
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

                return mapper.Map<MusicBrainzDiscViewModel>(result);
            }
            catch (Exception ex)
            {
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzReleaseViewModel> GetReleaseById(Guid musicBrainzReleaseId)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IRelease, MusicBrainzReleaseViewModel>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Artists |
                              Include.Recordings |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupReleaseAsync(musicBrainzReleaseId, include);

                return mapper.Map<MusicBrainzReleaseViewModel>(result);
            }
            catch (Exception ex)
            {
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzRecordingViewModel> GetRecordingById(Guid musicBrainzRecordingId)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IRecording, MusicBrainzRecordingViewModel>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Artists |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupRecordingAsync(musicBrainzRecordingId, include);

                return mapper.Map<MusicBrainzRecordingViewModel>(result);
            }
            catch (Exception ex)
            {
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        public async Task<MusicBrainzUrlViewModel> GetUrlById(Guid musicBrainzUrlId)
        {
            try
            {
                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var config = new MapperConfiguration(cfg => cfg.CreateMap<IUrl, MusicBrainzUrlViewModel>(MemberList.Destination));
                var mapper = config.CreateMapper();

                var include = Include.Artists |
                              Include.Media |
                              Include.Genres |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var result = await query.LookupUrlAsync(musicBrainzUrlId, include);

                return mapper.Map<MusicBrainzUrlViewModel>(result);
            }
            catch (Exception ex)
            {
                RaiseLog("Music Brainz Client Error:  {0}", LogMessageType.General, LogLevel.Error, ex.Message);
            }

            return null;
        }

        private void RaiseLog(string message, LogMessageType type, LogLevel level, params object[] parameters)
        {
            _outputController.AddLog(message, type, level, parameters);
        }
    }
}
