using AudioStation.Core.Database;
using AudioStation.Core.Model;
using AudioStation.Core.Model.M3U;

using M3UStream = AudioStation.Core.Model.M3U.M3UStream;

namespace AudioStation.Core.Component.Interface
{
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
        void AddUpdateLibraryEntry(string fileName, TagLib.File tagRef);

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
        /// Requests a page of data from the database
        /// </summary>
        /// <typeparam name="TEntity">The specific entity type</typeparam>
        PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : class;
    }
}
