using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor;

namespace AudioStation.Core.Component.Vendor.Interface
{
    public interface IMusicBrainzClient
    {
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
        Task<MusicBrainzRelease> GetReleaseById(Guid musicBrainzReleaseId);

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
