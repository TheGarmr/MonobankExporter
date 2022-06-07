using MonobankExporter.BusinessLogic.Models;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMetricsExporterService
    {
        public void ObserveAccount(AccountInfoModel account, double balance);
        public void ObserveCurrency(string currencyNameA, string currencyNameB, CurrencyObserveType type, float value);
    }
}