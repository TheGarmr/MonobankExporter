using Monobank.Core.Services;

namespace Monobank.Core
{
    public interface IMonoClient
    {
        MonobankCurrencyClient Currency { get; }
        MonobankServiceClient Client { get; }
    }
}