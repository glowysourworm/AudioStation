using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderEntityLoad<TEntity> : LibraryLoaderLoadBase where TEntity : AudioStationEntityBase
    {
        public TEntity Entity { get; private set; }

        public LibraryLoaderEntityLoad(TEntity entity)
        {
            this.Entity = entity;
        }
    }
}
