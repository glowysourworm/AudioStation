using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderEntityLoad<TEntity> : ILibraryLoaderLoad where TEntity : AudioStationEntityBase
    {
        public int OwnerId { get; private set; }
        public LibraryLoadType LoadType { get; private set; }
        public TEntity Entity { get; private set; }

        public LibraryLoaderEntityLoad(int ownerId, LibraryLoadType loadType, TEntity entity)
        {
            this.OwnerId = ownerId;
            this.Entity = entity;
            this.LoadType = loadType;
        }
    }
}
