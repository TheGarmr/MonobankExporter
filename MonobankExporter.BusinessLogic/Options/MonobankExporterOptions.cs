using System.Collections.Generic;

namespace MonobankExporter.BusinessLogic.Options
{
    public class MonobankExporterOptions
    {
        private int _clientsRefreshTimeInMinutes = 60;
        private int _currenciesRefreshTimeInMinutes = 10;

        public List<ClientInfoOptions> Clients { get; set; } = new();
        public string WebhookUrl { get; set; }
        public string ApiBaseUrl { get; set; } = "https://api.monobank.ua/";

        public int ClientsRefreshTimeInMinutes
        {
            get => _clientsRefreshTimeInMinutes;
            set => _clientsRefreshTimeInMinutes = value < 10 ? 10 : value;
        }
        public int CurrenciesRefreshTimeInMinutes
        {
            get => _currenciesRefreshTimeInMinutes;
            set => _currenciesRefreshTimeInMinutes = value < 10 ? 10 : value;
        }
    }
}