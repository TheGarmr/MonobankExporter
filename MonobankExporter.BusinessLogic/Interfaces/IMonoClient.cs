namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMonoClient
    {
        IMonobankCurrencyClient Currency { get; }
        IMonobankServiceClient Client { get; }
    }
}