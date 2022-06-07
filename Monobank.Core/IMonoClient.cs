using MonobankExporter.Client.Services;

namespace MonobankExporter.Client
{
    public interface IMonoClient
    {
        MonobankCurrencyClient Currency { get; }
        MonobankServiceClient Client { get; }
    }
}