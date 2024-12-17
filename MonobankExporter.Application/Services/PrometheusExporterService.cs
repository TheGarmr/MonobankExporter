using System.Globalization;
using MonobankExporter.Application.Enums;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Models;
using MonobankExporter.Application.Options;
using Prometheus;

namespace MonobankExporter.Application.Services;

public class PrometheusExporterService : IMetricsExporterService
{
    private readonly MetricsOptions _options;
    private Gauge _balanceGauge;
    private Gauge _jarsGauge;
    private Gauge _creditLimitGauge;
    private Gauge _currenciesBuyGauge;
    private Gauge _currenciesSellGauge;
    private Gauge _currenciesCrossGauge;

    public PrometheusExporterService(MetricsOptions options)
    {
        _options = options;
        SetupMetrics();
    }

    public void ObserveAccountBalance(AccountInfo account)
    {
        _balanceGauge
            .Labels(account.HolderName, account.CurrencyType, account.CardType)
            .Set(account.Balance);
        _creditLimitGauge
            .Labels(account.HolderName, account.CurrencyType, account.CardType)
            .Set(account.CreditLimit);
    }

    public void ObserveJarInfo(JarInfo jar)
    {
        _jarsGauge
            .Labels(jar.HolderName, jar.Title, jar.Description, jar.CurrencyType, jar.Goal?.ToString(CultureInfo.InvariantCulture) ?? string.Empty)
            .Set(jar.Balance);
    }

    public void ObserveCurrency(string currencyNameA, string currencyNameB, CurrencyObserveType type, float value)
    {
        switch (type)
        {
            case CurrencyObserveType.Buy:
                _currenciesBuyGauge.Labels(currencyNameA, currencyNameB).Set(value);
                break;
            case CurrencyObserveType.Sell:
                _currenciesSellGauge.Labels(currencyNameA, currencyNameB).Set(value);
                break;
            case CurrencyObserveType.Cross:
                _currenciesCrossGauge.Labels(currencyNameA, currencyNameB).Set(value);
                break;
        }
    }

    private void SetupMetrics()
    {
        _balanceGauge = Metrics.CreateGauge(_options.Balance, "shows current balance", new GaugeConfiguration
        {
            LabelNames = new[] { "name", "currency_type", "card_type" }
        });
        _jarsGauge = Metrics.CreateGauge(_options.Jars, "shows current jars", new GaugeConfiguration
        {
            LabelNames = new[] { "name", "title", "description", "currency_type", "goal" }
        });
        _creditLimitGauge = Metrics.CreateGauge(_options.CreditLimit, "shows current credit limit", new GaugeConfiguration
        {
            LabelNames = new[] { "name", "currency_type", "card_type" }
        });
        _currenciesBuyGauge = Metrics.CreateGauge(_options.CurrenciesBuy, "shows current rate for buy", new GaugeConfiguration
        {
            LabelNames = new[] { "currency_a", "currency_b" }
        });
        _currenciesSellGauge = Metrics.CreateGauge(_options.CurrenciesSell, "shows current rate for sell", new GaugeConfiguration
        {
            LabelNames = new[] { "currency_a", "currency_b" }
        });
        _currenciesCrossGauge = Metrics.CreateGauge(_options.CurrenciesCross, "shows current cross rate", new GaugeConfiguration
        {
            LabelNames = new[] { "currency_a", "currency_b" }
        });
    }
}