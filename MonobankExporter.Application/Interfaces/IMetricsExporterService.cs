using MonobankExporter.Application.Enums;
using MonobankExporter.Application.Models;

namespace MonobankExporter.Application.Interfaces;

public interface IMetricsExporterService
{
    public void ObserveAccountBalance(AccountInfo account);
    public void ObserveJarInfo(JarInfo jar);
    public void ObserveCurrency(string currencyNameA, string currencyNameB, CurrencyObserveType type, float value);
}