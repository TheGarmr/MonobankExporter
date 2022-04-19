using System;
using System.Threading;
using System.Threading.Tasks;

namespace MonobankExporter.API.Interfaces
{
    public interface IRedisCacheService
    {
        Task SetRecordAsync(string key, string data, DateTimeOffset absoluteExpireTime, CancellationToken cancellationToken);
        Task<string> GetRecordAsync(string key, CancellationToken cancellationToken);
    }
}