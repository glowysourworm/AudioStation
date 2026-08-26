using AudioStation.Core.Database.AudioStationDatabase;

namespace AudioStation.Core.Component.LibraryLoaderComponent.Output
{
    public class LibraryLoaderEntitySetOutput<T> where T : AudioStationEntityBase
    {
        List<T> _entities;

        public IEnumerable<T> Entities
        {
            get { return _entities; }
        }

        public LibraryLoaderEntitySetOutput()
        {
            _entities = new List<T>();
        }

        public void Add(T entity)
        {
            _entities.Add(entity);
        }
    }
}
