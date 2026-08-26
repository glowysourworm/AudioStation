using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Load
{
    public class LibraryLoaderEntitySetLoad<TEntity> where TEntity : AudioStationEntityBase
    {
        public IEnumerable<TEntity> EntitySet { get; private set; }

        public LibraryLoaderEntitySetLoad(IEnumerable<TEntity> entitySet)
        {
            this.EntitySet = entitySet;
        }
    }
}
