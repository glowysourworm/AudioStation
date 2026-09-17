using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderEntitySetLoad<TEntity> : ILibraryLoaderLoad where TEntity : AudioStationEntityBase
    {
        public LibraryLoadType Type { get; private set; }
        public IEnumerable<TEntity> EntitySet { get; private set; }

        public LibraryLoaderEntitySetLoad(LibraryLoadType loadType, IEnumerable<TEntity> entitySet)
        {
            this.EntitySet = entitySet;
            this.Type = loadType;
        }
    }
}
