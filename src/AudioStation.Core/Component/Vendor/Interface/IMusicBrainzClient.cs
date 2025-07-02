using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model.Vendor;

using MetaBrainz.MusicBrainz.CoverArt;
using MetaBrainz.MusicBrainz.Interfaces.Entities;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IMusicBrainzClient
    {


        // Entity Calls (Query and Cache)
        Task<IEnumerable<IArtist>> QueryArtists(string artist, int minScore);
        Task<IEnumerable<IRecording>> QueryRecordings(string artist, string album, string trackName, int minScore);
        Task<IEnumerable<IRelease>> QueryReleases(string artist, string album, string trackName, int minScore);
        Task<IEnumerable<IDisc>> QueryDiscs(string artist, string album, string trackName, int minScore);
        Task<IEnumerable<IGenre>> GetAllGenres();
        Task<IEnumerable<ITag>> GetAllTags();
        Task<IEnumerable<ILabel>> QueryLabel(string artist, string album, string trackName, int minScore);
        Task<IEnumerable<IMedium>> QueryMedia(string artist, string album, string trackName, int minScore);
        Task<IEnumerable<IUrl>> GetRelatedUrls(string artist, string album, string trackName, int minScore);

        /// <summary>
        /// Returns cover art for the release. This includes data.
        /// </summary>
        Task<IEnumerable<MusicBrainzPicture>> GetCoverArt(Guid musicBrainzReleaseId);

        /// <summary>
        /// Tries to look up information for the provided library entry
        /// </summary>
        Task<IEnumerable<MusicBrainzRecording>> Query(string artist, string album, string trackName, int minScore);

        /// <summary>
        /// Tries to lookup artist detail using the artist name
        /// </summary>
        Task<IEnumerable<MusicBrainzArtist>> QueryArtist(string artistName);

        /// <summary>
        /// Gathers information for the combined view model - which should be for a single track. The track name is provided in case the
        /// Id on the tag is out of date.
        /// </summary>
        Task<MusicBrainzCombined> GetCombinedData(string artistName, string albumName, string trackName);

        /// <summary>
        /// Gathers information for the combined view model - which should be for a single track. The track name is provided in case the
        /// Id on the tag is out of date.
        /// </summary>
        Task<MusicBrainzCombined> GetCombinedData(Guid releaseId, Guid artistId, Guid trackId, string trackName);

        /// <summary>
        /// Finds recording for a track (this would be to locate an Mp3FileReference in MusicBrainz)
        /// </summary>
        Task<MusicBrainzTrack> GetTrack(Guid trackId, string trackName, string albumName, string artistName, int searchScoreMin);

        /// <summary>
        /// Finds recording for a track (this would be to locate an Mp3FileReference in MusicBrainz)
        /// </summary>
        Task<MusicBrainzTrack> GetTrack(string trackName, string albumName, string artistName, int searchScoreMin);

        /// <summary>
        /// Gets a single artist record by Id
        /// </summary>
        Task<MusicBrainzArtist> GetArtistById(Guid musicBrainzArtistId);

        /// <summary>
        /// Gets a single disc record by Id
        /// </summary>
        Task<MusicBrainzDisc> GetDiscById(Guid musicBrainzDiscId);

        /// <summary>
        /// Gets a single release record by Id
        /// </summary>
        Task<IRelease> GetReleaseById(Guid musicBrainzReleaseId);

        /// <summary>
        /// Gets a release group by ID
        /// </summary>
        Task<IReleaseGroup> GetReleaseGroupById(Guid musicBrainzReleaseGroupId);

        /// <summary>
        /// Gets a single recording record by Id
        /// </summary>
        Task<MusicBrainzRecording> GetRecordingById(Guid musicBrainzRecordingId);

        /// <summary>
        /// Gets a single url record by Id
        /// </summary>
        Task<MusicBrainzUrl> GetUrlById(Guid musicBrainzUrlId);
    }
}
