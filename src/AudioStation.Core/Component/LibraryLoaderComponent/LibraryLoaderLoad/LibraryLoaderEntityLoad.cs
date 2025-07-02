using AudioStation.Core.Database.AudioStationDatabase;

using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Component.LibraryLoaderComponent.LibraryLoaderLoad
{
    public class LibraryLoaderEntityLoad : LibraryLoaderLoadBase
    {
        SimpleDictionary<int, AudioStationEntityBase> _entities;
        SimpleDictionary<int, bool> _entityResults;

        object _lock = new object();

        public LibraryLoaderEntityLoad(IEnumerable<AudioStationEntityBase> entities)
        {
            _entities = new SimpleDictionary<int, AudioStationEntityBase>();
            _entityResults = new SimpleDictionary<int, bool>();

            foreach (var entity in entities)
            {
                _entities.Add(entity.Id, entity);
            }
        }

        public void SetResult(AudioStationEntityBase entity, bool success)
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

        public IEnumerable<AudioStationEntityBase> GetPendingEntities()
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

        public override int GetFailureCount()
        {
            lock (_lock)
            {
                return _entityResults.Values.Count(x => !x);
            }
        }

        public override int GetSuccessCount()
        {
            lock (_lock)
            {
                return _entityResults.Values.Count(x => x);
            }
        }
    }
}
