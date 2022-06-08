using MonobankExporter.Client.Services;

namespace MonobankExporter.Client
{
    public interface IMonoClient
    {
        IMonobankCurrencyClient Currency { get; }
        IMonobankServiceClient Client { get; }
    }
}