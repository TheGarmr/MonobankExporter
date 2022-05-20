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

        public async Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
        {

        }

        public void Remove(string key)
        {

        }

        public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
        {

        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {

        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
            CancellationToken token = new CancellationToken())
        {

        }
    }
}