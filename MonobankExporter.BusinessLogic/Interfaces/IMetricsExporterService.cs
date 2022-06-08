using MonobankExporter.Domain.Enums;
using MonobankExporter.Domain.Models;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMetricsExporterService
    {
        public void ObserveAccount(AccountInfo account, double balance);
        public void ObserveCurrency(string currencyNameA, string currencyNameB, CurrencyObserveType type, float value);
    }
}