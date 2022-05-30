using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Monobank.Core;
using Monobank.Core.Models;
using MonobankExporter.API.Interfaces;
using MonobankExporter.API.Models;
using Newtonsoft.Json;

namespace MonobankExporter.API.Services
{
    public class MonobankService : IMonobankService
    {
        private readonly MonoClient _client;
        private readonly MonobankExporterOptions _options;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IPrometheusExporterService _prometheusExporter;

        public MonobankService(MonobankExporterOptions options, IPrometheusExporterService prometheusExporter, IRedisCacheService redisCacheService)
        {
            _client = new MonoClient();
            _options = options;
            _prometheusExporter = prometheusExporter;
            _redisCacheService = redisCacheService;
        }

        public async Task ExportUsersMetrics(bool webhookWillBeUsed, CancellationToken stoppingToken)
        {
            foreach (var clientInfo in _options.Clients)
            {
                await ExportMetricsForUser(webhookWillBeUsed, clientInfo, stoppingToken);
            }
        }

        public async Task SetupWebHookForUsers(string webHookUrl, CancellationToken stoppingToken)
        {
            try
            {
                foreach (var client in _options.Clients.Select(clientInfo => new MonoClient(clientInfo.Token)))
                {
                    await client.Client.SetWebhookAsync(webHookUrl, stoppingToken);
                }

                LogMessage("The setup of the webhook was successful.");
            }
            catch
            {
                LogMessage($"The setup of the webhook for users unexpectedly failed. Url: {webHookUrl}");
            }
        }

        public async Task ExportCurrenciesMetrics(CancellationToken stoppingToken)
        {
            try
            {
                var currencies = await _client.Currency.GetCurrencies(stoppingToken);

                var currenciesToObserve = currencies.Where(x =>
                    !string.IsNullOrWhiteSpace(x.CurrencyNameA) && !string.IsNullOrWhiteSpace(x.CurrencyNameB));

                foreach (var currency in currenciesToObserve)
                {
                    if (currency.RateBuy > 0)
                    {
                        _prometheusExporter.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB, CurrencyObserveType.Buy, currency.RateBuy);
                    }
                    if (currency.RateSell > 0)
                    {
                        _prometheusExporter.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB, CurrencyObserveType.Sell, currency.RateSell);
                    }
                    if (currency.RateCross > 0)
                    {
                        _prometheusExporter.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB, CurrencyObserveType.Cross, currency.RateCross);
                    }
                }
                LogMessage("Observed currencies metrics...");
            }
            catch
            {
                LogMessage("Observing of currencies metrics unexpectedly failed.");
            }
        }

        public async Task ExportMetricsForWebHook(WebHookModel webhook, CancellationToken stoppingToken)
        {
            LogMessage($"A webHook received. Card: {webhook?.Data?.Account}...");
            try
            {
                if (string.IsNullOrWhiteSpace(webhook?.Data?.Account) || webhook.Data?.StatementItem == null)
                {
                    LogMessage($"The webHook has invalid data. Metrics won't be exposed. Card: {webhook?.Data?.Account}...");
                    return;
                }

                var cacheRecord = await _redisCacheService.GetRecordAsync(webhook.Data.Account, stoppingToken);

                var accountInfo = JsonConvert.DeserializeObject<AccountInfoModel>(cacheRecord);
                if (accountInfo == null)
                {
                    LogMessage($"The cache doesn't contain a record with account info. Metrics won't be exposed. Card: {webhook.Data.Account}...");
                    return;
                }

                _prometheusExporter.ObserveAccount(accountInfo, webhook.Data.StatementItem.BalanceAsMoney - accountInfo.CreditLimit);
                LogMessage($"Observed metrics by webhook for {accountInfo.HolderName}...");
            }
            catch
            {
                LogMessage("Observing of currencies from webhook unexpectedly failed.");
            }
        }

        private async Task ExportMetricsForUser(bool webHookWillBeUsed, ClientInfoOptions clientInfo, CancellationToken stoppingToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientInfo?.Token))
                {
                    return;
                }

                var client = new MonoClient(clientInfo.Token);
                var userInfo = await client.Client.GetClientInfoAsync(stoppingToken);
                if (!string.IsNullOrWhiteSpace(clientInfo.Name))
                {
                    userInfo.Name = clientInfo.Name;
                }

                foreach (var account in userInfo.Accounts)
                {
                    var accountInfo = new AccountInfoModel
                    {
                        HolderName = userInfo.Name,
                        CurrencyType = account.CurrencyName,
                        CardType = account.Type.ToString(),
                        CreditLimit = account.CreditLimitAsMoney
                    };
                    var cacheRecord = JsonConvert.SerializeObject(accountInfo);
                    _prometheusExporter.ObserveAccount(accountInfo, account.BalanceWithoutCreditLimit);

                    if (webHookWillBeUsed)
                    {
                        await _redisCacheService.SetRecordAsync(account.Id, cacheRecord, DateTime.UtcNow.AddMinutes(_options.ClientsRefreshTimeInMinutes + 1), stoppingToken);
                    }
                }
                LogMessage($"Observed metrics for {userInfo.Name}");
            }
            catch
            {
                LogMessage($"Observing of metrics for {clientInfo?.Name} unexpectedly failed.");
            }
        }

        public bool WebHookUrlIsValid(string webHookUrl)
        {
            if (string.IsNullOrWhiteSpace(webHookUrl))
            {
                LogMessage("The webhook url is empty.");

                return false;
            }

            LogMessage($"Validating webhook url from configs: {webHookUrl}.");

            var isUrl = Uri.TryCreate(webHookUrl, UriKind.Absolute, out var uriResult);
            if (!isUrl)
            {
                LogMessage("The webhook url has bad format.");

                return false;
            }

            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                LogMessage("The webhook url does not contain HTTP or HTTPS.");

                return false;
            }

            if (!uriResult.AbsoluteUri.Contains("/webhook"))
            {
                LogMessage("The webhook url does not contain the '/webhook' path.");

                return false;
            }

            LogMessage("Webhook url is valid. Webhook and Redis will be used.");

            return true;
        }

        private static void LogMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }
    }
}