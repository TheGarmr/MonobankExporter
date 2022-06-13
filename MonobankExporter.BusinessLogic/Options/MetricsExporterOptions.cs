namespace MonobankExporter.BusinessLogic.Options
{
    public class MetricsExporterOptions
    {
        public string BalanceGaugeMetricName { get; set; } = "monobank_balance";
        public string CreditLimitGaugeMetricName { get; set; } = "monobank_credit_limit";
        public string CurrenciesBuyMetricMetricName { get; set; } = "monobank_currencies_buy";
        public string CurrenciesSellMetricMetricName { get; set; } = "monobank_currencies_sell";
        public string CurrenciesCrossMetricName { get; set; } = "monobank_currencies_cross";
    }
}
