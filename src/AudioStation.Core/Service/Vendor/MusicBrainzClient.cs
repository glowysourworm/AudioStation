using System.IO;

using ATL;

using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor.ATLExtension;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Payload;
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
        private uint _throttleLimitMilliseconds;
        private DateTime _lastServiceCall;
        private const int SERVICE_WAIT_MILLISEC = 100;
        private const int FIND_LIMIT = 10;

        [IocImportingConstructor]
        public MusicBrainzClient()
        {
            _status = IAudioStationService.Status.Disabled;
            _throttleLimitMilliseconds = 3000;
            _lastServiceCall = DateTime.MinValue;
        }

        protected void ServiceWait()
        {
            // Throttle limit for service calls
            while (DateTime.Now < _lastServiceCall.AddMilliseconds(_throttleLimitMilliseconds))
            {
                Thread.Sleep(SERVICE_WAIT_MILLISEC);
            }

            // UPDATE SERVICE WAIT
            _lastServiceCall = DateTime.Now;
        }

        protected async Task<IRecording?> RecordingQuery(Guid recordingId)
        {
            ServiceWait();

            try
            {
                OnStatusChanged(IAudioStationService.Status.Working);

                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var result = await query.LookupRecordingAsync(recordingId, CreateIncludeRecording());

                OnStatusChanged(IAudioStationService.Status.Idle);

                return result;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationService.Status.Error);

                throw new Exception("Music Brainz Client Error", ex);
            }
        }

        protected async Task<IRelease?> ReleaseQuery(Guid releaseId)
        {
            ServiceWait();

            try
            {
                OnStatusChanged(IAudioStationService.Status.Working);

                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var result = await query.LookupReleaseAsync(releaseId, CreateIncludeRelease());

                OnStatusChanged(IAudioStationService.Status.Idle);

                return result;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationService.Status.Error);

                throw new Exception("Music Brainz Client Error", ex);
            }
        }

        private async Task<IRecording?> FindTrack(string artist, string album, string title, Include include)
        {
            ServiceWait();

            try
            {
                OnStatusChanged(IAudioStationService.Status.Working);

                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var searchResults = await query.FindRecordingsAsync(string.Format("title:{0} artist:{1} release:{2}", title, artist, album));

                if (searchResults.Results.Count > 1)
                    ApplicationHelpers.Log("Music Brainz artist/album/title search is returning more than one result with 100% score:  {0}/{1}/{2}", LogMessageServiceType.MusicBrainz, LogLevel.Warning, null, artist, album, title);

                OnStatusChanged(IAudioStationService.Status.Idle);

                return searchResults.Results
                                    .Where(result => result.Score >= 100)
                                    .FirstOrDefault()?.Item;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationService.Status.Error);

                throw new Exception("Music Brainz Client Error", ex);
            }
        }

        protected async Task<CoverArtImage?> FrontArtQuery(Guid releaseId)
        {
            ServiceWait();

            try
            {
                // Query Release
                var release = await ReleaseQuery(releaseId);

                OnStatusChanged(IAudioStationService.Status.Working);

                var client = new CoverArt();
                var art = await client.FetchFrontAsync(release.Id);

                OnStatusChanged(IAudioStationService.Status.Idle);

                return art;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationService.Status.Error);

                throw new Exception("Music Brainz Client Error", ex);
            }
        }

        protected async Task<CoverArtImage?> BackArtQuery(Guid releaseId)
        {
            ServiceWait();

            try
            {
                // Query Release
                var release = await ReleaseQuery(releaseId);

                OnStatusChanged(IAudioStationService.Status.Working);

                var client = new CoverArt();
                var art = await client.FetchBackAsync(release.Id);

                OnStatusChanged(IAudioStationService.Status.Idle);

                return art;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationService.Status.Error);

                throw new Exception("Music Brainz Client Error", ex);
            }
        }

        protected bool Authenticate()
        {
            // There is no application identification for MusicBrainz, so most of the normal 
            // authentication data is not required to establish a connection. We'll just run
            // a simple query to verify

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
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationService.Status.Error);

                return false;
            }
        }

        #region (private) Release ID Lookup
        private Include CreateIncludeRecording()
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
        private Include CreateIncludeRelease()
        {
            // This should be broken down based on what is needed in the tag. Performance should vary depending on
            // how many fields, or what the lookup takes for field combinations.
            //
            return Include.Artists |
                   Include.Genres |
                   Include.Tags |
                   Include.Labels |
                   Include.Media;
        }
        private IAudioStationTag MapRecording(IRecording recording, IRelease release)
        {
            // This release will have track information
            var recordingRelease = recording.Releases?.FirstOrDefault(x => x.Date == release.Date);

            // Lookup media from the IRecording release
            var media = recordingRelease?.Media?.FirstOrDefault(x => x.Tracks.Any(z => z.Title == recording.Title));

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
                DiscTotal = (ushort)(recordingRelease?.Media?.Count ?? 0),
                Duration = recording.Length ?? TimeSpan.Zero,

                // IRecording.Release -> Genre (or) IRelease -> Genre (or) IArtist -> Genre
                Genre = recordingRelease?.Genres?.FirstOrDefault()?.Name ?? release?.Genres?.FirstOrDefault()?.Name ?? artist?.Artist?.Genres?.FirstOrDefault()?.Name ?? string.Empty,
                MediaFormat = media?.Format ?? string.Empty,
                Publisher = release?.LabelInfo?.FirstOrDefault()?.Label?.Name ?? string.Empty,
                TrackNumber = track?.Number ?? string.Empty,
                TrackTotal = (ushort)(media?.TrackCount ?? 0),
                Title = track?.Title ?? string.Empty,
                Track = (uint)(track?.Position ?? 0),
                Year = release?.Date?.Year ?? recordingRelease?.Date?.Year ?? track?.Recording?.FirstReleaseDate?.Year ?? 0
            };

            //var viewModel = new MusicBrainzCombined()
            //{
            //    ArtistId = artistId,
            //    Annotation = track.Recording?.Annotation ?? string.Empty,
            //    ArtistCreditName = track.Recording?.ArtistCredit?.FirstOrDefault()?.Name ?? artist.Name ?? string.Empty,
            //    Asin = release.Asin ?? string.Empty,
            //    FrontCover = null,
            //    BackCover = null,
            //    AssociatedUrls = release.Relationships?
            //                            .Where(x => x.TargetType == EntityType.Url)?
            //                            .Select(x => x.Url?.Resource?.AbsoluteUri ?? string.Empty)?
            //                            .ToList() ?? Enumerable.Empty<string>(),

            //    Barcode = release.Barcode ?? string.Empty,
            //    Disambiguation = track.Recording?.Disambiguation ?? string.Empty,
            //    Genres = track.Recording?.Genres?.Select(x => x.Name ?? string.Empty)?.ToList() ?? Enumerable.Empty<string>(),
            //    LabelCatalogNumber = release.LabelInfo?.FirstOrDefault()?.CatalogNumber ?? string.Empty,
            //    LabelCode = release.LabelInfo?.FirstOrDefault()?.Label?.LabelCode ?? 0,
            //    LabelCountry = release.LabelInfo?.FirstOrDefault()?.Label?.Country ?? string.Empty,
            //    LabelIpis = release.LabelInfo?.FirstOrDefault()?.Label?.Ipis ?? Enumerable.Empty<string>(),
            //    LabelName = release.LabelInfo?.FirstOrDefault()?.Label?.Name ?? string.Empty,
            //    MediumDiscCount = media.Discs?.Count ?? 0,
            //    MediumFormat = media.Format ?? string.Empty,
            //    MediumTitle = media.Title ?? string.Empty,
            //    MediumDiscPosition = mediaIndex + 1,
            //    MediumTrackCount = media.TrackCount,
            //    MediumTrackOffset = media.TrackOffset ?? 0,
            //    Packaging = release.Packaging ?? string.Empty,
            //    Quality = release.Quality ?? string.Empty,
            //    ReleaseCountry = release.Country ?? string.Empty,
            //    ReleaseDate = release.Date?.NearestDate ?? DateTime.MinValue,
            //    ReleaseId = release.Id,
            //    ReleaseStatus = release.Status ?? string.Empty,
            //    ReleaseTitle = release.Title ?? string.Empty,
            //    Tags = track.Recording?.Tags?.Select(x => x.Name)?.ToList() ?? Enumerable.Empty<string>(),
            //    Title = track.Title ?? string.Empty,
            //    Track = track,
            //    TrackId = track.Id,
            //    UserGenres = track.Recording?.UserGenres?.Select(x => x.Name ?? string.Empty)?.ToList() ?? Enumerable.Empty<string>(),
            //    UserTags = track.Recording?.UserTags?.Select(x => x.Name)?.ToList() ?? Enumerable.Empty<string>(),
            //};
        }
        private async Task<PictureInfo?> LookupArtMusicBrainzId(Guid recordingId, bool frontOrBack)
        {
            var recording = await RecordingQuery(recordingId);

            if (recording == null)
                return null;

            var release = recording.Releases?.FirstOrDefault(x => x.Date == recording.FirstReleaseDate);

            if (release == null)
                return null;

            var art = frontOrBack ? await FrontArtQuery(release.Id) : await BackArtQuery(release.Id);

            if (art == null)
                return null;

            using (var streamReader = new BinaryReader(art.Data))
            {
                art.Data.Position = 0;

                var binaryData = streamReader.ReadBytes((int)art.Data.Length);
                var pictureInfo = PictureInfo.fromBinaryData(binaryData, PictureInfo.PIC_TYPE.Front);

                art.Dispose();

                return pictureInfo;
            }
        }

        private async Task<IAudioStationTag?> LookupByArtistAlbumTitle(AudioStationTagServiceRequest serviceModel)
        {
            var recording = await FindTrack(serviceModel.Artist, serviceModel.Album, serviceModel.Title, CreateIncludeRecording());

            if (recording == null)
                return null;

            var releaseDate = recording?.FirstReleaseDate;
            var releaseId = recording?.Releases?.FirstOrDefault(x => x.Date == releaseDate)?.Id;

            if (releaseId == null)
                return null;

            var release = await ReleaseQuery((Guid)releaseId);

            if (release == null)
                return null;

            return MapRecording(recording, release);
        }
        private async Task<IAudioStationTag?> LookupByMusicBrainzId(AudioStationTagServiceRequest serviceModel)
        {
            var recording = await RecordingQuery(serviceModel.MusicBrainzRecordingId);

            if (recording == null)
                return null;

            var releaseDate = recording.FirstReleaseDate;
            var releaseId = recording.Releases?.FirstOrDefault(x => x.Date == releaseDate)?.Id;

            if (releaseId == null)
                return null;

            var release = await ReleaseQuery((Guid)releaseId);

            if (release == null)
                return null;

            return MapRecording(recording, release);
        }
        private async Task<AudioStationTagServiceResponse> Lookup(AudioStationTagServiceRequest serviceModel)
        {
            IAudioStationTag? result = null;

            // -> Music Brainz
            switch (serviceModel.IdType)
            {
                case AudioStationTagIdentity.ArtistAlbumTitle:
                    result = await LookupByArtistAlbumTitle(serviceModel);
                    break;
                case AudioStationTagIdentity.MusicBrainzId:
                    result = await LookupByMusicBrainzId(serviceModel);
                    break;
                default:
                    throw new Exception("Unhandled AudioStationTagIdentity type");
            }

            return new AudioStationTagServiceResponse(new TagPayload(result), result != null, result != null ? "Music Brainz client successful" : "Music Brainz client error");
        }
        private async Task<AudioStationTagServiceResponse> LookupSmall(AudioStationTagServiceRequest serviceModel)
        {
            IAudioStationTag? result = null;
            TagSmall tagSmall = null;
            string message = string.Empty;

            // -> Music Brainz
            switch (serviceModel.IdType)
            {
                case AudioStationTagIdentity.ArtistAlbumTitle:
                    result = await LookupByArtistAlbumTitle(serviceModel);
                    break;
                case AudioStationTagIdentity.MusicBrainzId:
                    result = await LookupByMusicBrainzId(serviceModel);
                    break;
                default:
                    throw new Exception("Unhandled AudioStationTagIdentity type");
            }

            // -> Map
            if (result != null)
                tagSmall = TagMapper.MapTo(result, VendorNames.MusicBrainz, serviceModel.MusicBrainzRecordingId);
            else
                tagSmall = null;

            return new AudioStationTagServiceResponse(new TagSmallPayload(tagSmall), tagSmall != null, tagSmall != null ? "Music Brainz client successful" : "Music Brainz client error");
        }
        private async Task<AudioStationTagServiceResponse> LookupArt(AudioStationTagServiceRequest request)
        {
            var pictureInfo = await LookupArtMusicBrainzId(request.MusicBrainzRecordingId, request.Type == AudioStationTagRequestType.ArtworkFront);

            return new AudioStationTagServiceResponse(new ArtworkPayload(pictureInfo), pictureInfo != null, pictureInfo != null ? "Music Brainz client successful" : "Music Brainz client error");
        }
        #endregion

        #region (public) IAudioStationTagService
        public Task<AudioStationTagServiceResponse> ProcessRequestAsync(AudioStationTagServiceRequest request)
        {
            return Task.Run(async () =>
            {
                switch (request.Type)
                {
                    case AudioStationTagRequestType.Tag:
                        return await Lookup(request);
                    case AudioStationTagRequestType.TagSmall:
                        return await LookupSmall(request);
                    case AudioStationTagRequestType.ArtworkFront:
                    case AudioStationTagRequestType.ArtworkBack:
                        return await LookupArt(request);
                    default:
                        throw new Exception("Unhandled service request type");
                }
            });
        }
        public AudioStationTagServiceResponse ProcessRequest(AudioStationTagServiceRequest request)
        {
            switch (request.Type)
            {
                case AudioStationTagRequestType.Tag:
                    return Lookup(request).Result;
                case AudioStationTagRequestType.TagSmall:
                    return LookupSmall(request).Result;
                case AudioStationTagRequestType.ArtworkFront:
                case AudioStationTagRequestType.ArtworkBack:
                    return LookupArt(request).Result;
                default:
                    throw new Exception("Unhandled service request type");
            }
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
        public IAudioStationService.Status Initialize(AudioStationConfiguration configuration)
        {
            //_client = Authenticate();

            return _status;
        }

        public Task<IAudioStationService.Status> InitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.Run(() => Initialize(configuration));
        }

        public IAudioStationService.Status ReInitialize(AudioStationConfiguration configuration)
        {
            return IAudioStationService.Status.Idle;
        }

        public Task<IAudioStationService.Status> ReInitializeAsync(AudioStationConfiguration configuration)
        {
            return Task.FromResult(IAudioStationService.Status.Idle);
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
