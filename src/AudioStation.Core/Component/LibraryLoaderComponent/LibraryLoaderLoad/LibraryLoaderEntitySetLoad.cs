using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderEntitySetLoad<TEntity> : LibraryLoaderLoadBase where TEntity : AudioStationEntityBase
    {
        public IEnumerable<TEntity> EntitySet { get; private set; }

        public LibraryLoaderEntitySetLoad(IEnumerable<TEntity> entitySet)
        {
            this.EntitySet = entitySet;
        }
    }
}
