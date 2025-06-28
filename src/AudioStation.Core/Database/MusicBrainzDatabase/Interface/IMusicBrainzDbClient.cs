using AudioStation.Core.Model;

namespace AudioStation.Core.Database.MusicBrainzDatabase.Interface
{
    public interface IMusicBrainzDbClient
    {
        /// <summary>
        /// Updates entity using property reflection
        /// </summary>
        bool UpdateEntity<TEntity>(TEntity entity) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Requests a page of data from the database
        /// </summary>
        /// <typeparam name="TEntity">The specific entity type</typeparam>
        PageResult<TEntity> GetPage<TEntity, TOrder>(PageRequest<TEntity, TOrder> request) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Gets an entire entity table from the database
        /// </summary>
        IEnumerable<TEntity> GetEntities<TEntity>() where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Gets entity by ID from the database
        /// </summary>
        TEntity? GetEntity<TEntity>(Guid musicBrainzId) where TEntity : MusicBrainzEntityBase;
    }
}
