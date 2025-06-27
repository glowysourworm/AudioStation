using AudioStation.Core.Database;

using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Component.LibraryLoaderComponent
{
    public class LibraryLoaderEntityLoad : LibraryLoaderLoadBase
    {
        SimpleDictionary<int, EntityBase> _entities;
        SimpleDictionary<int, bool> _entityResults;

        object _lock = new object();

        public LibraryLoaderEntityLoad(IEnumerable<EntityBase> entities)
        {
            _entities = new SimpleDictionary<int, EntityBase>();
            _entityResults = new SimpleDictionary<int, bool>();

            foreach (var entity in entities)
            {
                _entities.Add(entity.Id, entity);
            }
        }

        public void SetResult(EntityBase entity, bool success)
        {
            lock (_lock)
            {
                if (!_entityResults.ContainsKey(entity.Id))
                {
                    _entityResults[entity.Id] = success;
                }
                else
                    throw new Exception("Entity result set more than once:  LibraryLoaderEntityLoad.cs");
            }
        }

        public IEnumerable<EntityBase> GetPendingEntities()
        {
            lock (_lock)
            {
                return _entities.Values;
            }
        }

        public override double GetProgress()
        {
            lock (_lock)
            {
                return _entityResults.Count / (double)_entities.Count;
            }
        }

        public override bool HasErrors()
        {
            lock (_lock)
            {
                return _entityResults.Any(x => !x.Value);
            }
        }
    }
}
