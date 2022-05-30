using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Monobank.Core;
using Monobank.Core.Models;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.BusinessLogic.Models;
using Newtonsoft.Json;

namespace MonobankExporter.BusinessLogic.Services
{
    public class MonobankService : IMonobankService
    {
        private readonly MonoClient _client;
        private readonly MonobankExporterOptions _options;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IPrometheusExporterService _prometheusExporter;
        private readonly ILogger<MonobankService> _logger;

        public MonobankService(MonobankExporterOptions options,
            IPrometheusExporterService prometheusExporter,
            IRedisCacheService redisCacheService,
            ILogger<MonobankService> logger)
        {
            _client = new MonoClient();
            _options = options;
            _prometheusExporter = prometheusExporter;
            _redisCacheService = redisCacheService;
            _logger = logger;
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

                _logger.LogInformation("The setup of the webhook was successful.");
            }
            catch
            {
                _logger.LogError($"The setup of the webhook for users unexpectedly failed. Url: {webHookUrl}");
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
                _logger.LogInformation("Observed currencies metrics...");
            }
            catch
            {
                _logger.LogError("Observing of currencies metrics unexpectedly failed.");
            }
        }

        public async Task ExportMetricsForWebHook(WebHookModel webhook, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"A webHook received. Card: {webhook?.Data?.Account}...");
            try
            {
                if (string.IsNullOrWhiteSpace(webhook?.Data?.Account) || webhook.Data?.StatementItem == null)
                {
                    _logger.LogInformation($"The webHook has invalid data. Metrics won't be exposed. Card: {webhook?.Data?.Account}...");
                    return;
                }

                var cacheRecord = await _redisCacheService.GetRecordAsync(webhook.Data.Account, stoppingToken);

                var accountInfo = JsonConvert.DeserializeObject<AccountInfoModel>(cacheRecord);
                if (accountInfo == null)
                {
                    _logger.LogWarning($"The cache doesn't contain a record with account info. Metrics won't be exposed. Card: {webhook.Data.Account}...");
                    return;
                }

                _prometheusExporter.ObserveAccount(accountInfo, webhook.Data.StatementItem.BalanceAsMoney - accountInfo.CreditLimit);
                _logger.LogInformation($"Observed metrics by webhook for {accountInfo.HolderName}...");
            }
            catch
            {
                _logger.LogError("Observing of currencies from webhook unexpectedly failed.");
            }
        }

        private async Task ExportMetricsForUser(bool webHookWillBeUsed, ClientInfoOptions clientInfo, CancellationToken stoppingToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientInfo?.Token))
                {
                    _logger.LogError($"Could not expose metrics for client: {clientInfo?.Name}. Token is not empty.");
                    return;
                }

                var client = new MonoClient(clientInfo.Token);
                var userInfo = await client.Client.GetClientInfoAsync(stoppingToken);
                if (!string.IsNullOrWhiteSpace(clientInfo.Name))
                {
                    _logger.LogInformation($"Client named as {userInfo.Name} will be displayed as {clientInfo.Name}");
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
                _logger.LogInformation($"Observed metrics for {userInfo.Name}");
            }
            catch
            {
                _logger.LogError($"Observing of metrics for {clientInfo?.Name} unexpectedly failed.");
            }
        }

        public bool WebHookUrlIsValid(string webHookUrl)
        {
            if (string.IsNullOrWhiteSpace(webHookUrl))
            {
                _logger.LogWarning("The webhook url is empty.");

                return false;
            }

            _logger.LogInformation($"Validating webhook url from configs: {webHookUrl}.");

            var isUrl = Uri.TryCreate(webHookUrl, UriKind.Absolute, out var uriResult);
            if (!isUrl)
            {
                _logger.LogWarning("The webhook url has bad format.");

                return false;
            }

            if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            {
                _logger.LogWarning("The webhook url does not contain HTTP or HTTPS.");

                return false;
            }

            if (!uriResult.AbsoluteUri.EndsWith("/webhook"))
            {
                _logger.LogWarning("The webhook url does not contain the '/webhook' path.");

                return false;
            }

            return true;
        }
    }
}