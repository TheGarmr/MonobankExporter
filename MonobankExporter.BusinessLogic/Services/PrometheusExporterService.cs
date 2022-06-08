using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.Domain.Enums;
using MonobankExporter.Domain.Models;
using Prometheus;

namespace MonobankExporter.BusinessLogic.Services
{
    public class PrometheusExporterService : IMetricsExporterService
    {
        private readonly Gauge _balanceGauge = Metrics.CreateGauge("monobank_balance", "shows current balance", new GaugeConfiguration
        {
            LabelNames = new[] { "name", "currency_type", "card_type" }
        });
        private readonly Gauge _creditLimitGauge = Metrics.CreateGauge("monobank_credit_limit", "shows current credit limit", new GaugeConfiguration
        {
            LabelNames = new[] { "name", "currency_type", "card_type" }
        });
        private readonly Gauge _currenciesBuyGauge = Metrics.CreateGauge("monobank_currencies_buy", "shows current rate for buy", new GaugeConfiguration
        {
            LabelNames = new[] { "currency_a", "currency_b" }
        });
        private readonly Gauge _currenciesSellGauge = Metrics.CreateGauge("monobank_currencies_sell", "shows current rate for sell", new GaugeConfiguration
        {
            LabelNames = new[] { "currency_a", "currency_b" }
        });
        private readonly Gauge _currenciesCrossGauge = Metrics.CreateGauge("monobank_currencies_cross", "shows current cross rate", new GaugeConfiguration
        {
            LabelNames = new[] { "currency_a", "currency_b" }
        });

        public void ObserveAccount(AccountInfo account, double balance)
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
    }
}
