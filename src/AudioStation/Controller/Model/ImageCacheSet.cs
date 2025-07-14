using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Controller.Model
{
    public class ImageCacheSet<TType, TKey, TValue> where TValue : IDisposable
    {
        SimpleDictionary<TType, ImageCache<TKey, TValue>> _cache;

        public ImageCacheSet()
        {
            _cache = new SimpleDictionary<TType, ImageCache<TKey, TValue>>();
        }

        public ImageCache<TKey, TValue> GetCache(TType type)
        {
            return _cache[type];
        }

        public bool Contains(TType type, TKey key)
        {
            return _cache[type].Contains(key);
        }

        public void AddCache(TType type, ImageCache<TKey, TValue> cache)
        {
            _cache.Add(type, cache);
        }
    }
}
