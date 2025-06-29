using AudioStation.Core.Model;

namespace AudioStation.Core.Database.MusicBrainzDatabase.Interface
{
    public interface IMusicBrainzDbClient
    {
        /// <summary>
        /// Adds Url + Related Entity mapping for a music brainz Url
        /// </summary>
        void AddUrlRelationship<TEntity>(MusicBrainzUrlEntity entity, TEntity relatedEntity) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Adds Genre + Related Entity mapping for a music brainz Url
        /// </summary>
        void AddGenreRelationship<TEntity>(MusicBrainzGenreEntity entity, TEntity relatedEntity) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Adds Tag + Related Entity mapping for a music brainz Url
        /// </summary>
        void AddTagRelationship<TEntity>(MusicBrainzTagEntity entity, TEntity relatedEntity) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Adds entity using the DbContext (generic method)
        /// </summary>
        void AddEntity<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Returns true if the database contains specified entity (by ID)
        /// </summary>
        bool ContainsEntity<TEntity>(TEntity entity) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Returns true if there are any entities that pass the given predicate
        /// </summary>
        bool Any<TEntity>(Func<TEntity, bool> predicate) where TEntity : MusicBrainzEntityBase;

        /// <summary>
        /// Returns subset of entity set that passes the given predicate
        /// </summary>
        IEnumerable<TEntity> Where<TEntity>(Func<TEntity, bool> predicate) where TEntity : class;

        /// <summary>
        /// Updates entity using generic context method (property reflection)
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
