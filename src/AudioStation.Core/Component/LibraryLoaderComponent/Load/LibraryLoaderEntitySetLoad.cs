using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderEntitySetLoad<TEntity> : ILibraryLoaderLoad where TEntity : AudioStationEntityBase
    {
        public int OwnerId { get; private set; }
        public LibraryLoadType LoadType { get; private set; }
        public IEnumerable<TEntity> EntitySet { get; private set; }

        public LibraryLoaderEntitySetLoad(int ownerId, LibraryLoadType loadType, IEnumerable<TEntity> entitySet)
        {
            this.OwnerId = ownerId;
            this.EntitySet = entitySet;
            this.LoadType = loadType;
        }
    }
}
