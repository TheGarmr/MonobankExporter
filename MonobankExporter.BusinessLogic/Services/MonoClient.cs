using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.Domain.Options;

namespace MonobankExporter.BusinessLogic.Services
{
    public class MonoClient : IMonoClient
    {
        private const string ResponseMediaType = "application/json";

        public IMonobankCurrencyClient Currency { get; }
        public IMonobankServiceClient Client { get; }

        public MonoClient(MonobankExporterOptions options, ILogger<MonoClient> logger)
        {
            if (string.IsNullOrWhiteSpace(options.ApiBaseUrl))
            {
                logger.LogCritical("Critical error: ApiBaseUrl config is not provided.");
                throw new ArgumentException("Critical error: ApiBaseUrl config is not provided.", nameof(options.ApiBaseUrl));
            }

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ResponseMediaType));
            httpClient.BaseAddress = new Uri(options.ApiBaseUrl);

            Currency = new MonobankCurrencyClient(httpClient);
            Client = new MonobankServiceClient(httpClient);
        }
    }
}
