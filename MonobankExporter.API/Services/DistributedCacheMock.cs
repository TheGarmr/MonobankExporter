using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace MonobankExporter.API.Services
{
    public class DistributedCacheMock : IDistributedCache
    {
        public byte[] Get(string key)
        {
            return new byte[] { };
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = new CancellationToken())
        {
            return await Task.FromResult(new byte[] { });
        }

        public void Refresh(string key)
        {

        }

        public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {

        }

        public Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {

        }

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }
}