using AudioStation.Core.Database.AudioStationDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase;
using AudioStation.Core.Database.MusicBrainzDatabase.Model;
using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

using MetaBrainz.MusicBrainz.Interfaces.Entities;

using M3UStream = AudioStation.Core.Model.M3U.M3UStream;

namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Multi-thread safe component that handles database communications. The logging is sent to the
    /// dispatcher from this component - on this component's code. 
    /// </summary>
    public interface IModelController
    {
        #region Audio Station Database

        /// <summary>
        /// Adds LibraryEntry to database. Does NOT update any existing, similar, entry. The tag data
        /// is also used to initialize the LibraryEntry, adding supporting data to the database.
        /// </summary>

        Mp3FileReference AddUpdateLibraryEntry(string fileName, IAudioStationTag tagRef);

        /// <summary>
        /// Add / Update M3UStream based on unique Id, and Name
        /// </summary>
        bool AddUpdateRadioEntry(M3UStream entry);

        /// <summary>
        /// (Batch) Adds M3UStreams to database based on unique Id, and Name
        /// </summary>
        bool AddRadioEntries(IEnumerable<M3UStream> entries);

        /// <summary>
        /// Returns all files associated with the artist
        /// </summary>
        /// <param name="artistId">Database id of the artist</param>
        IEnumerable<Mp3FileReference> GetArtistFiles(int artistId);

        /// <summary>
        /// Returns all albums associated with this artist
        /// </summary>
        IEnumerable<Mp3FileReferenceAlbum> GetArtistAlbums(int artistId, bool isPrimaryArtist);

        /// <summary>
        /// Returns tracks associated with an album
        /// </summary>
        IEnumerable<Mp3FileReference> GetAlbumTracks(int albumId);

        #endregion

        #region Music Brainz Local / Remote (Query / Cache)

        MusicBrainzArtistEntity AddUpdateMusicBrainzArtist(IArtist musicBrainzArtist);
        MusicBrainzRecordingEntity AddUpdateMusicBrainzRecording(IRecording musicBrainzRecording);
        MusicBrainzReleaseEntity AddUpdateMusicBrainzRelease(IRelease musicBrainzRelease);
        MusicBrainzTrackEntity AddUpdateMusicBrainzTrack(ITrack musicBrainzTrack);
        MusicBrainzTagEntity AddUpdateMusicBrainzTag(ITag musicBrainzTag, MusicBrainzEntityBase musicBrainzEntity);
        MusicBrainzGenreEntity AddUpdateMusicBrainzGenre(IGenre musicBrainzGenre, MusicBrainzEntityBase musicBrainzEntity);
        MusicBrainzUrlEntity AddUpdateMusicBrainzUrl(IUrl musicBrainzUrl, MusicBrainzEntityBase musicBrainzEntity);
        MusicBrainzMediumEntity AddUpdateMusicBrainzMedium(Guid musicBrainzReleaseId, IMedium musicBrainzMedium);
        MusicBrainzLabelEntity AddUpdateMusicBrainzLabel(ILabel musicBrainzLocal);

        /// <summary>
        /// Get music brainz data - checking locally and remotely depending on what information is given. If you
        /// supply a UUID (Guid), then the first check uses a local Guid check. Then, a remote Guid check, then a remote
        /// find. Any data that comes back from a remote find is stored locally.
        /// </summary>
        MusicBrainzArtistEntity GetMusicBrainzArtist(Guid? musicBrainzId, string artistName, string albumName, string trackName);

        /// <summary>
        /// Get music brainz data - checking locally and remotely depending on what information is given. If you
        /// supply a UUID (Guid), then the first check uses a local Guid check. Then, a remote Guid check, then a remote
        /// find. Any data that comes back from a remote find is stored locally.
        /// </summary>
        MusicBrainzRecordingEntity GetMusicBrainzRecording(Guid? musicBrainzId, string artistName, string albumName, string trackName);

        /// <summary>
        /// Get music brainz data - checking locally and remotely depending on what information is given. If you
        /// supply a UUID (Guid), then the first check uses a local Guid check. Then, a remote Guid check, then a remote
        /// find. Any data that comes back from a remote find is stored locally.
        /// </summary>
        MusicBrainzReleaseEntity GetMusicBrainzRelease(Guid? musicBrainzId, string artistName, string albumName, string trackName);

        /// <summary>
        /// Get music brainz data - checking locally and remotely depending on what information is given. If you
        /// supply a UUID (Guid), then the first check uses a local Guid check. Then, a remote Guid check, then a remote
        /// find. Any data that comes back from a remote find is stored locally.
        /// </summary>
        MusicBrainzTrackEntity GetMusicBrainzTrack(Guid? musicBrainzId, string artistName, string albumName, string trackName);

        /// <summary>
        /// Get music brainz tag data (all records) (storing locally which haven't been cached yet)
        /// </summary>
        IEnumerable<MusicBrainzTagEntity> GetMusicBrainzTags(MusicBrainzEntityBase relatedEntity);

        /// <summary>
        /// Get music brainz genre data (all records) (storing locally which haven't been cached yet)
        /// </summary>
        IEnumerable<MusicBrainzGenreEntity> GetMusicBrainzGenres(MusicBrainzEntityBase relatedEntity);

        /// <summary>
        /// Get music brainz data - checking locally and remotely depending on what information is given. If you
        /// supply a UUID (Guid), then the first check uses a local Guid check. Then, a remote Guid check, then a remote
        /// find. Any data that comes back from a remote find is stored locally.
        /// </summary>
        MusicBrainzLabelEntity GetMusicBrainzLabel(Guid? musicBrainzId, string artistName, string albumName, string trackName);

        /// <summary>
        /// Get music brainz data - checking locally and remotely depending on what information is given. If you
        /// supply a UUID (Guid), then the first check uses a local Guid check. Then, a remote Guid check, then a remote
        /// find. Any data that comes back from a remote find is stored locally.
        /// </summary>
        IEnumerable<MusicBrainzUrlEntity> GetMusicBrainzRelatedUrls<TEntity>(Guid musicBrainzEntityId, string artistName, string albumName, string trackName) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Get music brainz data - checking locally and remotely depending on what information is given. If you
        /// supply a UUID (Guid), then the first check uses a local Guid check. Then, a remote Guid check, then a remote
        /// find. Any data that comes back from a remote find is stored locally.
        /// </summary>
        IEnumerable<MusicBrainzMediumEntity> GetMusicBrainzMedia(Guid musicBrainzReleaseId, string artistName, string albumName, string trackName);

        /// <summary>
        /// Gets a complete record for one track - starting with the matching recordings - for the artist, album, track name search
        /// from the MusicBrainz database. This will store any data that returns locally for a single search to MB.
        /// </summary>
        IEnumerable<MusicBrainzCombinedLibraryEntryRecord> GetCompleteMusicBrainzRecord(string artistName, string albumName, string trackName);
        #endregion

        #region Generic Queries (both DbContext)

        /// <summary>
        /// Updates entity using property reflection
        /// </summary>
        bool UpdateMusicBrainzEntity<TEntity>(TEntity entity) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Requests a page of data from the database
        /// </summary>
        /// <typeparam name="TEntity">The specific entity type</typeparam>
        PageResult<TEntity> GetMusicBrainzPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Gets an entire entity table from the database
        /// </summary>
        IEnumerable<TEntity> GetMusicBrainzEntities<TEntity>() where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Gets entity by ID from the database
        /// </summary>
        TEntity GetMusicBrainzEntity<TEntity>(Guid musicBrainzId) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Updates entity using property reflection
        /// </summary>
        bool UpdateAudioStationEntity<TEntity>(TEntity entity) where TEntity : AudioStationEntityBase;

        /// <summary>
        /// Requests a page of data from the database
        /// </summary>
        /// <typeparam name="TEntity">The specific entity type</typeparam>
        PageResult<TEntity> GetAudioStationPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : AudioStationEntityBase;

        /// <summary>
        /// Gets an entire entity table from the database
        /// </summary>
        IEnumerable<TEntity> GetAudioStationEntities<TEntity>() where TEntity : AudioStationEntityBase;

        /// <summary>
        /// Gets entity by ID from the database
        /// </summary>
        TEntity GetAudioStationEntity<TEntity>(int id) where TEntity : AudioStationEntityBase;

        #endregion
    }
}
