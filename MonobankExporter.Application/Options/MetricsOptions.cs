namespace MonobankExporter.Application.Options
{
    public class MetricsOptions
    {
        public string Balance { get; set; } = "monobank_balance";
        public string CreditLimit { get; set; } = "monobank_credit_limit";
        public string CurrenciesBuy { get; set; } = "monobank_currencies_buy";
        public string CurrenciesSell { get; set; } = "monobank_currencies_sell";
        public string CurrenciesCross { get; set; } = "monobank_currencies_cross";
    }
}
