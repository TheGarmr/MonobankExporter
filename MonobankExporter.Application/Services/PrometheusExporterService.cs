using MonobankExporter.Application.Enums;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Models;
using MonobankExporter.Application.Options;
using Prometheus;

namespace MonobankExporter.Application.Services
{
    public class PrometheusExporterService : IMetricsExporterService
    {
        private readonly MetricsExporterOptions _options;
        private Gauge _balanceGauge;
        private Gauge _creditLimitGauge;
        private Gauge _currenciesBuyGauge;
        private Gauge _currenciesSellGauge;
        private Gauge _currenciesCrossGauge;

        public PrometheusExporterService(MetricsExporterOptions options)
        {
            _options = options;
            SetupMetrics();
        }

        public void ObserveAccountBalance(AccountInfo account, double balance)
        {
            _balanceGauge.Labels(account.HolderName, account.CurrencyType, account.CardType).Set(balance);
            _creditLimitGauge.Labels(account.HolderName, account.CurrencyType, account.CardType).Set(account.CreditLimit);
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
            _balanceGauge = Metrics.CreateGauge(_options.BalanceGaugeMetricName, "shows current balance", new GaugeConfiguration
            {
                LabelNames = new[] { "name", "currency_type", "card_type" }
            });
            _creditLimitGauge = Metrics.CreateGauge(_options.CreditLimitGaugeMetricName, "shows current credit limit", new GaugeConfiguration
            {
                LabelNames = new[] { "name", "currency_type", "card_type" }
            });
            _currenciesBuyGauge = Metrics.CreateGauge(_options.CurrenciesBuyMetricMetricName, "shows current rate for buy", new GaugeConfiguration
            {
                LabelNames = new[] { "currency_a", "currency_b" }
            });
            _currenciesSellGauge = Metrics.CreateGauge(_options.CurrenciesSellMetricMetricName, "shows current rate for sell", new GaugeConfiguration
            {
                LabelNames = new[] { "currency_a", "currency_b" }
            });
            _currenciesCrossGauge = Metrics.CreateGauge(_options.CurrenciesCrossMetricName, "shows current cross rate", new GaugeConfiguration
            {
                LabelNames = new[] { "currency_a", "currency_b" }
            });
        }
    }
}
