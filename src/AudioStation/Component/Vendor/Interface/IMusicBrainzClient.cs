using AudioStation.Core.Model;
using AudioStation.ViewModels.Vendor.MusicBrainzViewModel;

namespace AudioStation.Component.Vendor.Interface
{
    public interface IMusicBrainzClient
    {
        /// <summary>
        /// Tries to look up information for the provided library entry
        /// </summary>
        Task<IEnumerable<MusicBrainzRecordingViewModel>> Query(LibraryEntry entry);

        /// <summary>
        /// Tries to lookup artist detail using the artist name
        /// </summary>
        Task<IEnumerable<MusicBrainzArtistViewModel>> QueryArtist(string artistName);

        /// <summary>
        /// Gathers information for the combined view model - which should be for a single track. The track name is provided in case the
        /// Id on the tag is out of date.
        /// </summary>
        Task<MusicBrainzCombinedViewModel> GetCombinedData(string artistName, string albumName, string trackName);

        /// <summary>
        /// Gathers information for the combined view model - which should be for a single track. The track name is provided in case the
        /// Id on the tag is out of date.
        /// </summary>
        Task<MusicBrainzCombinedViewModel> GetCombinedData(Guid releaseId, Guid artistId, Guid trackId, string trackName);

        /// <summary>
        /// Gets a single artist record by Id
        /// </summary>
        Task<MusicBrainzArtistViewModel> GetArtistById(Guid musicBrainzArtistId);

        /// <summary>
        /// Gets a single disc record by Id
        /// </summary>
        Task<MusicBrainzDiscViewModel> GetDiscById(Guid musicBrainzDiscId);

        /// <summary>
        /// Gets a single release record by Id
        /// </summary>
        Task<MusicBrainzReleaseViewModel> GetReleaseById(Guid musicBrainzReleaseId);

        /// <summary>
        /// Gets a single recording record by Id
        /// </summary>
        Task<MusicBrainzRecordingViewModel> GetRecordingById(Guid musicBrainzRecordingId);

        /// <summary>
        /// Gets a single url record by Id
        /// </summary>
        Task<MusicBrainzUrlViewModel> GetUrlById(Guid musicBrainzUrlId);
    }
}
