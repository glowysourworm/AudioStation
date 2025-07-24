using AutoMapper;

using SimpleWpf.RecursiveSerializer.Shared;
using SimpleWpf.SimpleCollections.Collection;

namespace AudioStation.Core.Utility
{
    internal static class ApplicationMapperCache
    {
        private static SimpleDictionary<int, IMapper> Cache;

        static ApplicationMapperCache()
        {
            Cache = new SimpleDictionary<int, IMapper>();
        }

        internal static void Set(Type sourceType, Type destinationType, IMapper mapper)
        {
            var hashCode = CreateHashCode(sourceType, destinationType);

            if (!Cache.ContainsKey(hashCode))
                Cache.Add(hashCode, mapper);
        }

        internal static bool Has(Type sourceType, Type destinationType)
        {
            var hashCode = CreateHashCode(sourceType, destinationType);

            return Cache.ContainsKey(hashCode);
        }

        internal static IMapper Get(Type sourceType, Type destinationType)
        {
            var hashCode = CreateHashCode(sourceType, destinationType);

            if (Cache.ContainsKey(hashCode))
                return Cache[hashCode];

            else
                throw new ArgumentException("IMapper instance not contained in the cache");
        }

        private static int CreateHashCode(Type sourceType, Type destinationType)
        {
            return RecursiveSerializerHashGenerator.CreateSimpleHash(sourceType, destinationType);
        }
    }
}
