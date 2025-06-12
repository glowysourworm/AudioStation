using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Controller.Model
{
    public class ImageCache<TKey, TValue>
    {
        public int CacheLimit { get; private set; }

        readonly SimpleDictionary<TKey, TValue> _cacheDict;

        public ImageCache(int cacheLimit)
        {
            this.CacheLimit = cacheLimit;
            _cacheDict = new SimpleDictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            if (_cacheDict.Count >= this.CacheLimit)
                _cacheDict.RemoveFirst();

            _cacheDict.Add(key, value);
        }

        public bool Contains(TKey key)
        {
            return _cacheDict.ContainsKey(key);
        }
        public TValue Get(TKey key)
        {
            return _cacheDict[key];
        }
    }
}
