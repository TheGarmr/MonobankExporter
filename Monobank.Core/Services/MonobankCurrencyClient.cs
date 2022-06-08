﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MonobankExporter.Client.Models;

namespace MonobankExporter.Client.Services
{
    public class MonobankCurrencyClient : IMonobankCurrencyClient
    {
        private const string CurrencyEndpoint = "bank/currency";
        private readonly HttpClient _httpClient;

        public MonobankCurrencyClient(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<ICollection<CurrencyInfo>> GetCurrenciesAsync(CancellationToken stoppingToken)
        {
            var uri = new Uri($"{CurrencyEndpoint}", UriKind.Relative);
            var response = await _httpClient.GetAsync(uri, stoppingToken);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<Error>(responseString);
                throw new Exception(error.Description);
            }

            return JsonSerializer.Deserialize<ICollection<CurrencyInfo>>(responseString);
        }
    }
}
