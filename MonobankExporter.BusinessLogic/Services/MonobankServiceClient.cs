using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.Domain.Extensions;
using MonobankExporter.Domain.Models.Client;

namespace MonobankExporter.BusinessLogic.Services
{
    public class MonobankServiceClient : IMonobankServiceClient
    {
        private const string ClientInfoEndpoint = "personal/client-info";
        private const string StatementEndpoint = "personal/statement";
        private const string WebhookEndpoint = "personal/webhook";
        private const string TokenHeader = "X-Token";
        private const int RequestLimit = 60; // seconds
        private const int MaxStatementRange = 2682000; // 31 day + 1 hour
        private readonly HttpClient _httpClient;
        private DateTime _previousRequestTimestamp = DateTime.UtcNow.AddSeconds(-RequestLimit);

        public MonobankServiceClient(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<UserInfo> GetClientInfoAsync(string token, CancellationToken cancellationToken)
        {
            _httpClient.DefaultRequestHeaders.Remove(TokenHeader);
            _httpClient.DefaultRequestHeaders.Add(TokenHeader, token);

            var uri = new Uri(ClientInfoEndpoint, UriKind.Relative);
            var response = await _httpClient.GetAsync(uri, cancellationToken);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<MonobankApiError>(responseString);
                throw new Exception(error.Description);
            }
            return JsonSerializer.Deserialize<UserInfo>(responseString);
        }

        public async Task<ICollection<Statement>> GetStatementsAsync(DateTime from, DateTime to, string account = "0")
        {
            if (to.ToUnixTime() - from.ToUnixTime() >= MaxStatementRange)
            {
                throw new Exception("Time range exceeded. Difference between 'from' and 'to' should be less than 31 day + 1 hour.");
            }

            if ((DateTime.UtcNow - _previousRequestTimestamp).TotalSeconds <= RequestLimit)
            {
                throw new Exception($"Request limit exceeded. Only 1 request per {RequestLimit} seconds allowed.");
            }

            var uri = new Uri($"{StatementEndpoint}/{account}/{from.ToUnixTime()}/{to.ToUnixTime()}", UriKind.Relative);
            var response = await _httpClient.GetAsync(uri);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                var error = JsonSerializer.Deserialize<MonobankApiError>(responseString);
                throw new Exception(error.Description);
            }
            _previousRequestTimestamp = DateTime.UtcNow;
            return JsonSerializer.Deserialize<ICollection<Statement>>(responseString);
        }

        public async Task<bool> SetWebhookAsync(string url, string token, CancellationToken stoppingToken)
        {
            _httpClient.DefaultRequestHeaders.Remove(TokenHeader);
            _httpClient.DefaultRequestHeaders.Add(TokenHeader, token);

            // create body containing webhook url
            var body = JsonSerializer.Serialize(new { webHookUrl = url });
            // uri to call
            var uri = new Uri(WebhookEndpoint, UriKind.Relative);
            // set webhook
            var response = await _httpClient.PostAsync(uri, new StringContent(body), stoppingToken);

            return response.IsSuccessStatusCode;
        }
    }
}