using MonobankExporter.Application.Enums;
using MonobankExporter.Application.Models;

namespace MonobankExporter.Application.Interfaces
{
    public interface IMetricsExporterService
    {
        public void ObserveAccount(AccountInfo account, double balance);
        public void ObserveCurrency(string currencyNameA, string currencyNameB, CurrencyObserveType type, float value);
    }
}