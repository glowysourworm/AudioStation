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

                var include = Include.Releases |
                              Include.Recordings |
                              Include.Media |
                              Include.RecordingRelationships |
                              Include.Annotation |
                              Include.LabelRelationships | 
                              Include.Genres |
                              Include.Tags |
                              Include.UrlRelationships |
                              Include.LabelRelationships |
                              Include.EventRelationships;

                var artist = await query.LookupArtistAsync(artistId, include);
                var release = await GetReleaseById(releaseId);
                var media = release.Media?.FirstOrDefault(x => x.Tracks?.Any(z => z.Id == trackId || z.Title == trackName) ?? false);

                var coverArtClient = new CoverArt();
                var frontArt = release.CoverArtArchive.Front ? coverArtClient.FetchFront(releaseId) : null;
                var backArt = release.CoverArtArchive.Back ? coverArtClient.FetchBack(releaseId) : null;

                var frontViewModel = frontArt != null ? new ImageViewModel(frontArt.Data, frontArt.ContentType) : null;
                var backViewModel = backArt != null ? new ImageViewModel(backArt.Data, backArt.ContentType) : null;

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
                var mediaIndex = release.Media.IndexOf(media);

                if (track == null)
                {
                    RaiseLog("Music Brainz Client failed to retrieve track for:  {0}", LogMessageType.General, LogLevel.Error, trackName);
                    return null;
                }

                var viewModel = new MusicBrainzCombinedViewModel()
                {
                    ArtistId = artistId,
                    Annotation = track.Recording.Annotation,
                    ArtistCreditName = track.Recording.ArtistCredit?.FirstOrDefault()?.Name,
                    Asin = release.Asin,
                    FrontCover = frontViewModel,
                    BackCover = backViewModel,
                    AssociatedUrls = artist.Relationships?.Where(x => x.TargetType == EntityType.Url)?.Select(x => x.Url?.Resource?.AbsoluteUri ?? string.Empty)?.ToList(),
                    Barcode = release.Barcode,
                    Disambiguation = track.Recording.Disambiguation,
                    Genres = track.Recording?.Genres?.Select(x => x.Name)?.ToList(),
                    LabelCatalogNumber = release.LabelInfo?.FirstOrDefault()?.CatalogNumber,
                    LabelCode = release.LabelInfo?.FirstOrDefault()?.Label?.LabelCode ?? 0,
                    LabelCountry = release.LabelInfo?.FirstOrDefault()?.Label?.Country,
                    LabelIpis = release.LabelInfo?.FirstOrDefault()?.Label?.Ipis,
                    LabelName = release.LabelInfo?.FirstOrDefault()?.Label?.Name,
                    MediumDiscCount = media.Discs?.Count ?? 0,
                    MediumFormat = media.Format,
                    MediumTitle = media.Title,
                    MediumDiscPosition = mediaIndex + 1,
                    MediumTrackCount = media.TrackCount,
                    MediumTrackOffset = media.TrackOffset ?? 0,
                    Packaging = release.Packaging,
                    Quality = release.Quality,
                    ReleaseCountry = release.Country,
                    ReleaseDate = release.Date?.NearestDate ?? DateTime.MinValue,
                    ReleaseId = release.Id,
                    ReleaseStatus = release.Status,
                    Tags = track.Recording?.Tags?.Select(x => x.Name)?.ToList(),
                    Title = track.Title,
                    Track = track,
                    TrackId = track.Id,
                    UserGenres = track.Recording?.UserGenres?.Select(x => x.Name)?.ToList(),
                    UserTags = track.Recording?.UserTags?.Select(x => x.Name)?.ToList(),
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
