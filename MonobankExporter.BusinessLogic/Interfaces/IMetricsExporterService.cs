using MonobankExporter.BusinessLogic.Enums;
using MonobankExporter.BusinessLogic.Models;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMetricsExporterService
    {
        public void ObserveAccount(AccountInfo account, double balance);
        public void ObserveCurrency(string currencyNameA, string currencyNameB, CurrencyObserveType type, float value);
    }
}