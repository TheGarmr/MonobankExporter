using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MonobankExporter.BusinessLogic.Interfaces;

namespace MonobankExporter.BusinessLogic.Services
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache _redisCache;

        public RedisCacheService(IDistributedCache redisCache)
        {
            _redisCache = redisCache;
        }

        public async Task SetRecordAsync(string key, string data, DateTimeOffset absoluteExpireTime, CancellationToken cancellationToken)
        {
            var options = new DistributedCacheEntryOptions
            {
                //AbsoluteExpiration = absoluteExpireTime
            };

            await _redisCache.SetStringAsync(key, data, options, cancellationToken);
        }

        public async Task<string> GetRecordAsync(string key, CancellationToken cancellationToken)
        {
            return await _redisCache.GetStringAsync(key, cancellationToken);
        }
    }
}