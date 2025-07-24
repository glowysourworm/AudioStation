using AudioStation.Core.Model;
using AudioStation.Core.Model.Vendor.ATLExtension.Interface;

namespace AudioStation.Core.Database.AudioStationDatabase.Interface
{
    public interface IAudioStationDbClient
    {
        /// <summary>
        /// Adds LibraryEntry to database. Does NOT update any existing, similar, entry. The tag data
        /// is also used to initialize the LibraryEntry, adding supporting data to the database.
        /// </summary>
        Mp3FileReference AddUpdateLibraryEntry(string fileName, bool fileAvailable, bool fileLoadError, string fileLoadErrorMessage, IAudioStationTag tagRef);

        /// <summary>
        /// Add / Update M3UStream based on unique Id, and Name
        /// </summary>
        void AddUpdateRadioEntry(Core.Model.M3U.M3UStream entry);

        /// <summary>
        /// (Batch) Adds M3UStreams to database based on unique Id, and Name
        /// </summary>
        void AddRadioEntries(IEnumerable<Core.Model.M3U.M3UStream> entries);

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

        /// <summary>
        /// Updates entity using property reflection
        /// </summary>
        bool UpdateEntity<TEntity>(TEntity entity) where TEntity : AudioStationEntityBase;

        /// <summary>
        /// Requests a page of data from the database
        /// </summary>
        /// <typeparam name="TEntity">The specific entity type</typeparam>
        PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : AudioStationEntityBase;

        /// <summary>
        /// Gets an entire entity table from the database
        /// </summary>
        IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : AudioStationEntityBase;

        /// <summary>
        /// Gets entity by ID from the database
        /// </summary>
        TEntity? GetEntity<TEntity>(int id) where TEntity : AudioStationEntityBase;
    }
}
