using System;
using System.Net.Http;
using System.Net.Http.Headers;
using MonobankExporter.Client.Services;

namespace MonobankExporter.Client
{
    public class MonoClient : IMonoClient
    {
        private const string BaseApiUrl = "https://api.monobank.ua/";
        private const string ResponseMediaType = "application/json";

        public MonobankCurrencyClient Currency { get; }
        public MonobankServiceClient Client { get; }

        public MonoClient()
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ResponseMediaType));
            httpClient.BaseAddress = new Uri(BaseApiUrl);

            Currency = new MonobankCurrencyClient(httpClient);
            Client = new MonobankServiceClient(httpClient);
        }
    }
}
