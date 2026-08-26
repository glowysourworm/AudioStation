using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderEntityLoad<TEntity> where TEntity : AudioStationEntityBase
    {
        public TEntity Entity { get; private set; }

        public LibraryLoaderEntityLoad(TEntity entity)
        {
            this.Entity = entity;
        }
    }
}
