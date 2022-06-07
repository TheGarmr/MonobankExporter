using Microsoft.Extensions.Caching.Memory;
using MonobankExporter.BusinessLogic.Models;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface ILookupsMemoryCache : IMemoryCache
    {
        bool TryGetValue<TItem>(CacheType cacheType, object? key, out TItem value);
        TItem Set<TItem>(CacheType cacheType, object key, TItem value, MemoryCacheEntryOptions options);
    }
}