using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MonobankExporter.Application.Enums;
using MonobankExporter.Application.Interfaces;

namespace MonobankExporter.Application.Services
{
    public class LookupsMemoryCache : MemoryCache, ILookupsMemoryCache
    {
        public LookupsMemoryCache(IOptions<MemoryCacheOptions> options) : base(options) { }

        public bool TryGetValue<TItem>(CacheType cacheType, object key, out TItem value)
        {
            return this.TryGetValue(CreateKey(cacheType, key), out value);
        }

        public TItem Set<TItem>(CacheType cacheType, object key, TItem value, MemoryCacheEntryOptions options)
        {
            return this.Set(CreateKey(cacheType, key), value, options);
        }

        private static string CreateKey<T>(CacheType cacheType, T key)
        {
            return (object.Equals(key, default(T)))
                ? $"{cacheType}"
                : $"{cacheType}:{key}";
        }
    }
}