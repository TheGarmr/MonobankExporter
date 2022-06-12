using System.Collections.Generic;

namespace MonobankExporter.BusinessLogic.Options
{
    public class MonobankExporterOptions
    {
        private int _clientsRefreshTimeInMinutes = 60;
        private int _currenciesRefreshTimeInMinutes = 10;

        public List<ClientInfoOptions> Clients { get; set; } = new List<ClientInfoOptions>();
        public string WebhookUrl { get; set; }
        public string ApiBaseUrl { get; set; } = "https://api.monobank.ua/";

        public int ClientsRefreshTimeInMinutes
        {
            get => _clientsRefreshTimeInMinutes;
            set
            {
                if (value < 2)
                {
                    value = 2;
                }

                _clientsRefreshTimeInMinutes = value;
            }
        }
        public int CurrenciesRefreshTimeInMinutes
        {
            get => _currenciesRefreshTimeInMinutes;
            set
            {
                if (value < 10)
                {
                    value = 10;
                }

                _currenciesRefreshTimeInMinutes = value;
            }
        }
    }
}