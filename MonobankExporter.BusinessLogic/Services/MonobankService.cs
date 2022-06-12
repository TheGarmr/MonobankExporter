﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Monobank.Client;
using Monobank.Client.Models;
using MonobankExporter.BusinessLogic.Enums;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.BusinessLogic.Models;
using MonobankExporter.BusinessLogic.Options;

namespace MonobankExporter.BusinessLogic.Services
{
    public class MonobankService : IMonobankService
    {
        private readonly IMonobankClient _monobankClient;
        private readonly ILookupsMemoryCache _cacheService;
        private readonly IMetricsExporterService _metricsExporter;
        private readonly ILogger<MonobankService> _logger;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public MonobankService(MonobankExporterOptions options,
            IMonobankClient monobankClient,
            IMetricsExporterService metricsExporterService,
            ILookupsMemoryCache cacheService,
            ILogger<MonobankService> logger)
        {
            _monobankClient = monobankClient;
            _metricsExporter = metricsExporterService;
            _cacheService = cacheService;
            _logger = logger;
            _cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(options.ClientsRefreshTimeInMinutes + 1)
            };
        }

        public async Task ExportMetricsForUsersAsync(bool storeToCache, List<ClientInfoOptions> clients, CancellationToken stoppingToken)
        {
            if (clients == null || !clients.Any())
            {
                _logger.LogWarning("List of clients is empty. Metrics could not be exported.");
                return;
            }

            foreach (var clientInfo in clients)
            {
                await ExportMetricsForUser(storeToCache, clientInfo, stoppingToken);
            }
        }

        public async Task SetupWebHookForUsersAsync(string webHookUrl, List<ClientInfoOptions> clients, CancellationToken stoppingToken)
        {
            if (clients == null || !clients.Any())
            {
                _logger.LogWarning("List of clients is empty. Webhook could not be set.");
                return;
            }

            try
            {
                foreach (var client in clients)
                {
                    await _monobankClient.SetWebhookAsync(webHookUrl, client.Token, stoppingToken);
                }

                _logger.LogInformation("The setup of the webhook was successful.");
            }
            catch
            {
                _logger.LogError($"The setup of the webhook for users unexpectedly failed. Url: {webHookUrl}");
            }
        }

        public async Task ExportMetricsForCurrenciesAsync(CancellationToken stoppingToken)
        {
            try
            {
                var currencies = await _monobankClient.GetCurrenciesAsync(stoppingToken);

                var currenciesToObserve = currencies.Where(x =>
                    !string.IsNullOrWhiteSpace(x.CurrencyNameA) && !string.IsNullOrWhiteSpace(x.CurrencyNameB));

                foreach (var currency in currenciesToObserve)
                {
                    if (currency.RateBuy > 0)
                    {
                        _metricsExporter.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB, CurrencyObserveType.Buy, currency.RateBuy);
                    }
                    if (currency.RateSell > 0)
                    {
                        _metricsExporter.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB, CurrencyObserveType.Sell, currency.RateSell);
                    }
                    if (currency.RateCross > 0)
                    {
                        _metricsExporter.ObserveCurrency(currency.CurrencyNameA, currency.CurrencyNameB, CurrencyObserveType.Cross, currency.RateCross);
                    }
                }
                _logger.LogInformation("Observed currencies metrics...");
            }
            catch
            {
                _logger.LogError("Observing of currencies metrics unexpectedly failed.");
            }
        }

        public void ExportMetricsOnWebHook(WebHook webhook, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"A webHook received. Card: {webhook?.Data?.Account}...");
            try
            {
                if (string.IsNullOrWhiteSpace(webhook?.Data?.Account) || webhook.Data?.StatementItem == null)
                {
                    _logger.LogInformation($"The webHook has invalid data. Metrics won't be exposed. Card: {webhook?.Data?.Account}...");
                    return;
                }

                if (!_cacheService.TryGetValue(CacheType.AccountInfo, webhook.Data.Account, out AccountInfo accountInfo))
                {
                    _logger.LogWarning($"The cache doesn't contain a record with account info. Metrics won't be exposed. Card: {webhook.Data.Account}...");
                    return;
                }

                _metricsExporter.ObserveAccount(accountInfo, webhook.Data.StatementItem.BalanceAsMoney - accountInfo.CreditLimit);
                _logger.LogInformation($"Observed metrics by webhook for {accountInfo.HolderName}...");
            }
            catch
            {
                _logger.LogError("Observing of currencies from webhook unexpectedly failed.");
            }
        }

        private async Task ExportMetricsForUser(bool storeToCache, ClientInfoOptions clientInfo, CancellationToken stoppingToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(clientInfo?.Token))
                {
                    _logger.LogError($"Could not expose metrics for client: {clientInfo?.Name}. Token is empty.");
                    return;
                }

                var userInfo = await _monobankClient.GetClientInfoAsync(clientInfo.Token, stoppingToken);
                if (!string.IsNullOrWhiteSpace(clientInfo.Name))
                {
                    _logger.LogTrace($"Client named as {userInfo.Name} will be displayed as {clientInfo.Name}");
                    userInfo.Name = clientInfo.Name;
                }

                foreach (var account in userInfo.Accounts)
                {
                    var accountInfo = new AccountInfo
                    {
                        HolderName = userInfo.Name,
                        CurrencyType = account.CurrencyName,
                        CardType = account.Type.ToString(),
                        CreditLimit = account.CreditLimitAsMoney
                    };

                    _metricsExporter.ObserveAccount(accountInfo, account.BalanceWithoutCreditLimit);
                    if (storeToCache)
                    {
                        _cacheService.Set(CacheType.AccountInfo, account.Id, accountInfo, _cacheOptions);
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

            if (!uriResult.AbsoluteUri.Contains("."))
            {
                _logger.LogWarning("The webhook url does not dot in the address. It seems like it's not a domain.");
                return false;
            }

            if (!uriResult.AbsoluteUri.EndsWith("/webhook"))
            {
                _logger.LogWarning("The webhook url does not ends with the '/webhook' path.");
                return false;
            }

            return true;
        }
    }
}