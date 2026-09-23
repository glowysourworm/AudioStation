using System.IO;

using ATL;

using AudioStation.Core.Model;
using AudioStation.Core.Model.Interface;
using AudioStation.Core.Service.Interface;
using AudioStation.Core.Service.Payload.Input;
using AudioStation.Core.Service.Payload.Interface;
using AudioStation.Core.Service.Payload.Output;
using AudioStation.Core.Service.Vendor.Interface;
using AudioStation.Core.Utility;

using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.CoverArt;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

using Microsoft.Extensions.Logging;

using SimpleWpf.Extensions;
using SimpleWpf.Extensions.Collection;
using SimpleWpf.IocFramework.Application.Attribute;

using Query = MetaBrainz.MusicBrainz.Query;

namespace AudioStation.Core.Service.Vendor
{
    [IocExport(typeof(IMusicBrainzClient))]
    public class MusicBrainzClient : VendorServiceBase, IMusicBrainzClient
    {
        // Music Brainz Query Syntax: https://musicbrainz.org/doc/MusicBrainz_API/Search
        //
        private const string QUERY_FORMAT = "{0}:{1}";
        private const string QUERY_ALBUM = "album";
        private const string QUERY_ARTIST = "artist";
        private const string QUERY_TITLE = "title";
        private const string QUERY_TRACK_ID = "tid";
        private const string QUERY_RECORDING_ID = "rid";

        [IocImportingConstructor]
        public MusicBrainzClient() : base("Music Brainz Client", "Music Brainz Client")
        {
        }

        protected string BuildQuery(params (string, string)[] parameters)
        {
            var query = parameters.Select(pair => string.Format(QUERY_FORMAT, pair.Item1, pair.Item2)).ToArray();

            // The separator character may be flexible. The space seems to work.
            return query.Join(" ");
        }

        protected async Task<IRecording?> RecordingQuery(MusicBrainzLookupPayload payload)
        {
            ServiceWait();

            try
            {
                OnStatusChanged(IAudioStationDataService.Status.Working);

                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();

                IRecording? result = null;

                switch (payload.IdType)
                {
                    case MusicBrainzLookupRequestType.ArtistAlbumTitle:
                        result = (await query.FindRecordingsAsync(BuildQuery((QUERY_ALBUM, payload.Album), (QUERY_ARTIST, payload.Artist), (QUERY_TITLE, payload.Title))))
                                           .Results
                                           .FirstOrDefault()
                                           ?.Item;
                        break;

                    case MusicBrainzLookupRequestType.MusicBrainzRecordingId:
                        result = await query.LookupRecordingAsync(payload.MusicBrainzId.Value, CreateIncludeRecording());
                        break;

                    case MusicBrainzLookupRequestType.MusicBrainzTrackId:
                    case MusicBrainzLookupRequestType.MusicBrainzReleaseTrackId:
                        result = (await query.FindRecordingsAsync(BuildQuery((QUERY_TRACK_ID, payload.MusicBrainzId.Value.ToString()))))
                                           .Results
                                           .FirstOrDefault()
                                           ?.Item;
                        break;
                    default:
                        throw new Exception("Unhandled Music Brainz Lookup ID Type");
                }

                OnStatusChanged(IAudioStationDataService.Status.Idle);

                return result;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationDataService.Status.Error);

                throw new Exception("Music Brainz Client Error", ex);
            }
        }

        protected async Task<IRelease?> ReleaseQuery(Guid releaseId)
        {
            ServiceWait();

            try
            {
                OnStatusChanged(IAudioStationDataService.Status.Working);

                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var result = await query.LookupReleaseAsync(releaseId, CreateIncludeRelease());

                OnStatusChanged(IAudioStationDataService.Status.Idle);

                return result;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationDataService.Status.Error);

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

                OnStatusChanged(IAudioStationDataService.Status.Working);

                var client = new CoverArt();
                var art = await client.FetchFrontAsync(release.Id);

                OnStatusChanged(IAudioStationDataService.Status.Idle);

                return art;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationDataService.Status.Error);

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

                OnStatusChanged(IAudioStationDataService.Status.Working);

                var client = new CoverArt();
                var art = await client.FetchBackAsync(release.Id);

                OnStatusChanged(IAudioStationDataService.Status.Idle);

                return art;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationDataService.Status.Error);

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
                OnStatusChanged(IAudioStationDataService.Status.Working);

                // Initialize MetaBrainz.MusicBrainz client
                var query = new Query();
                var searchResults = query.FindAllArtists("artist:Coldplay", 1);

                OnStatusChanged(IAudioStationDataService.Status.Idle);

                return true;
            }
            catch (Exception ex)
            {
                ApplicationHelpers.Log("Music Brainz Client Error:  {0}", LogMessageServiceType.MusicBrainz, LogLevel.Error, ex, ex.Message?.Trim() ?? string.Empty);

                OnStatusChanged(IAudioStationDataService.Status.Error);

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
        private ITagFull MapRecording(IRecording recording, IRelease release)
        {
            // This release will have track information
            var recordingRelease = recording.Releases?.FirstOrDefault(x => x.Date == release.Date);

            // Lookup media from the IRecording release
            var media = recordingRelease?.Media?.FirstOrDefault(x => x.Tracks.Any(z => z.Title == recording.Title));

            var track = media?.Tracks?.FirstOrDefault(x => x.Title == recording.Title);
            var artist = recording.ArtistCredit?.FirstOrDefault();
            var artistName = artist?.Name ?? artist?.Artist?.Name ?? string.Empty;

            return new TagFull()
            {
                Album = recordingRelease?.Title,
                AlbumArtist = artistName,
                Artist = artistName,
                Comment = null,
                Copyright = null,
                DurationMilliseconds = (int)(recording?.Length?.TotalMilliseconds ?? 0),
                Genre = recordingRelease?.Genres?.FirstOrDefault()?.Name ??
                        release?.Genres?.FirstOrDefault()?.Name ??
                        recording?.Genres?.FirstOrDefault()?.Name ??
                        recording?.UserGenres?.FirstOrDefault()?.Name ??
                        recordingRelease?.UserGenres?.FirstOrDefault()?.Name ??
                        release?.UserGenres?.FirstOrDefault()?.Name,
                MediaFormat = media?.Format,
                MediaNumber = media?.Position,
                MediaTotal = release?.Media.Count,
                Publisher = release?.LabelInfo.FirstOrDefault()?.Label?.Name,
                Title = recording?.Title,
                TrackNumber = track?.Position,
                TrackTotal = media?.TrackCount,
                Year = release?.Date?.Year
            };
            //return new AudioStationTag()
            //{
            //    Album = release?.Title ?? string.Empty,

            //    Artist = artistName,
            //    Date = recording.FirstReleaseDate?.NearestDate ?? DateTime.MinValue,
            //    AlbumArtist = artistName,
            //    AlbumArtists = recording.ArtistCredit?.Select(x => x.Name ?? x.Artist?.Name ?? string.Empty)?.ToList() ?? new List<string>(),
            //    DiscNumber = (ushort)(media?.Position ?? 0),
            //    DiscTotal = (ushort)(recordingRelease?.Media?.Count ?? 0),
            //    Duration = recording.Length ?? TimeSpan.Zero,

            //    // IRecording.Release -> Genre (or) IRelease -> Genre (or) IArtist -> Genre
            //    Genre = recordingRelease?.Genres?.FirstOrDefault()?.Name ?? release?.Genres?.FirstOrDefault()?.Name ?? artist?.Artist?.Genres?.FirstOrDefault()?.Name ?? string.Empty,
            //    MediaFormat = media?.Format ?? string.Empty,
            //    Publisher = release?.LabelInfo?.FirstOrDefault()?.Label?.Name ?? string.Empty,
            //    TrackNumber = track?.Number ?? string.Empty,
            //    TrackTotal = (ushort)(media?.TrackCount ?? 0),
            //    Title = track?.Title ?? string.Empty,
            //    Track = (uint)(track?.Position ?? 0),
            //    Year = release?.Date?.Year ?? recordingRelease?.Date?.Year ?? track?.Recording?.FirstReleaseDate?.Year ?? 0
            //};

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

        private async Task<AudioStationTagServiceResponse> Lookup(MusicBrainzLookupPayload payload, bool smallPayload)
        {
            var recording = await RecordingQuery(payload);

            if (recording == null)
                return null;

            var releaseDate = recording?.FirstReleaseDate;
            var releaseId = recording?.Releases?.FirstOrDefault(x => x.Date == releaseDate)?.Id;

            if (releaseId == null)
                return null;

            var release = await ReleaseQuery((Guid)releaseId);

            if (release == null)
                return null;

            ITagSmall? tagSmall = null;
            ITagFull tagFull = MapRecording(recording, release);

            // -> Map
            if (tagFull != null)
                tagSmall = TagMapper.Map(tagFull);
            else
                tagSmall = null;

            ITagServiceOutputPayload outputPayload = smallPayload ? new TagSmallPayload(tagSmall) : new TagPayload(tagFull);

            return new AudioStationTagServiceResponse(outputPayload, tagSmall != null, tagSmall != null ? "Music Brainz client successful" : "Music Brainz client error");
        }
        private async Task<AudioStationTagServiceResponse> LookupArt(MusicBrainzLookupPayload payload, AudioStationTagRequestType artRequestType)
        {
            var recording = await RecordingQuery(payload);

            if (recording == null)
                return null;

            var release = recording.Releases?.FirstOrDefault(x => x.Date == recording.FirstReleaseDate);

            if (release == null)
                return null;

            var art = artRequestType == AudioStationTagRequestType.ArtworkFront ? await FrontArtQuery(release.Id) : await BackArtQuery(release.Id);

            if (art == null)
                return null;

            PictureInfo? pictureInfo = null;

            using (var streamReader = new BinaryReader(art.Data))
            {
                art.Data.Position = 0;

                var binaryData = streamReader.ReadBytes((int)art.Data.Length);
                pictureInfo = PictureInfo.fromBinaryData(binaryData, PictureInfo.PIC_TYPE.Front);

                art.Dispose();
            }

            return new AudioStationTagServiceResponse(new ArtworkPayload(pictureInfo), pictureInfo != null, pictureInfo != null ? "Music Brainz client successful" : "Music Brainz client error");
        }
        #endregion

        #region (public) IAudioStationTagService
        public Task<AudioStationTagServiceResponse> ProcessRequestAsync(AudioStationTagServiceRequest request)
        {
            var inputPayload = request.Payload as MusicBrainzLookupPayload;

            if (inputPayload == null)
                throw new ArgumentException("Invalid Music Brainz Client Payload");

            return Task.Run(async () =>
            {
                switch (request.Type)
                {
                    case AudioStationTagRequestType.Tag:
                    case AudioStationTagRequestType.TagSmall:
                        return await Lookup(inputPayload, request.Type == AudioStationTagRequestType.TagSmall);
                    case AudioStationTagRequestType.ArtworkFront:
                    case AudioStationTagRequestType.ArtworkBack:
                        return await LookupArt(inputPayload, request.Type);
                    default:
                        throw new Exception("Unhandled service request type");
                }
            });
        }
        public AudioStationTagServiceResponse ProcessRequest(AudioStationTagServiceRequest request)
        {
            var inputPayload = request.Payload as MusicBrainzLookupPayload;

            if (inputPayload == null)
                throw new ArgumentException("Invalid Music Brainz Client Payload");

            switch (request.Type)
            {
                case AudioStationTagRequestType.Tag:
                case AudioStationTagRequestType.TagSmall:
                    return Lookup(inputPayload, request.Type == AudioStationTagRequestType.TagSmall).Result;
                case AudioStationTagRequestType.ArtworkFront:
                case AudioStationTagRequestType.ArtworkBack:
                    return LookupArt(inputPayload, request.Type).Result;
                default:
                    throw new Exception("Unhandled service request type");
            }
        }
        #endregion

        #region (public) IAudioStationComponent Methods
        public override IAudioStationDataService.Status Initialize(AudioStationConfiguration configuration)
        {
            // Wait period between calls
            SetThrottleLimit((uint)configuration.MusicBrainzWaitMilliseconds);

            // -> Idle
            OnStatusChanged(IAudioStationDataService.Status.Idle);

            // -> Return Status
            return base.Initialize(configuration);
        }
        #endregion
    }
}
