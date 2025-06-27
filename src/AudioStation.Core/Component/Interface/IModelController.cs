using AudioStation.Core.Database;
using AudioStation.Core.Model;

using M3UStream = AudioStation.Core.Model.M3U.M3UStream;

namespace AudioStation.Core.Component.Interface
{
    /// <summary>
    /// Multi-thread safe component that handles database communications. The logging is sent to the
    /// dispatcher from this component - on this component's code. 
    /// </summary>
    public interface IModelController : IDisposable
    {
        /// <summary>
        /// Method to initialize the controller. This must be called prior to usage.
        /// </summary>
        //void Initialize();

        /// <summary>
        /// Collection of primary library of mp3 file references. The rest of the mp3 data may be
        /// loaded using the IModelController.
        /// </summary>
        //Library Library { get; }

        /// <summary>
        /// Collection of radio stream data. Additional data my be loaded using online data services.
        /// </summary>
        //Radio Radio { get; }

        /// <summary>
        /// Adds LibraryEntry to database. Does NOT update any existing, similar, entry. The tag data
        /// is also used to initialize the LibraryEntry, adding supporting data to the database.
        /// </summary>
        void AddUpdateLibraryEntry(string fileName, bool fileAvailable, bool fileLoadError, string fileLoadErrorMessage, TagLib.File tagRef);

        /// <summary>
        /// Add / Update M3UStream based on unique Id, and Name
        /// </summary>
        void AddUpdateRadioEntry(M3UStream entry);

        /// <summary>
        /// (Batch) Adds M3UStreams to database based on unique Id, and Name
        /// </summary>
        void AddRadioEntries(IEnumerable<M3UStream> entries);

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
        void UpdateEntity<TEntity>(TEntity entity) where TEntity : EntityBase;

        /// <summary>
        /// Requests a page of data from the database
        /// </summary>
        /// <typeparam name="TEntity">The specific entity type</typeparam>
        PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : EntityBase;

        /// <summary>
        /// Gets an entire entity table from the database
        /// </summary>
        IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : EntityBase;

        /// <summary>
        /// Gets entity by ID from the database
        /// </summary>
        TEntity GetEntity<TEntity>(int id) where TEntity : EntityBase;
    }
}
