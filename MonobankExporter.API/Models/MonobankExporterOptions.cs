﻿using System.Collections.Generic;

namespace MonobankExporter.API.Models
{
    public class MonobankExporterOptions
    {
        private int _clientsRefreshTimeInMinutes = 60;
        private int _currenciesRefreshTimeInMinutes = 10;

        public List<ClientInfoOptions> Clients { get; set; } = new();
        public string WebhookUrl { get; set; }
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