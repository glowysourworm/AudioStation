using AudioStation.Core.Component.LibraryLoaderComponent.Load.Interface;
using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderEntityLoad<TEntity> : ILibraryLoaderLoad where TEntity : AudioStationEntityBase
    {
        public LibraryLoadType Type { get; private set; }
        public TEntity Entity { get; private set; }

        public LibraryLoaderEntityLoad(LibraryLoadType loadType, TEntity entity)
        {
            this.Entity = entity;
            this.Type = loadType;
        }
    }
}
