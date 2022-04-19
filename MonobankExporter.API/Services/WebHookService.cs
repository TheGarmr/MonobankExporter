using System;
using System.Threading;
using System.Threading.Tasks;
using Monobank.Core.Models;
using MonobankExporter.API.Interfaces;
using MonobankExporter.API.Models;
using Newtonsoft.Json;

namespace MonobankExporter.API.Services
{
    public class WebHookService : IWebHookService
    {
        private readonly IPrometheusExporterService _prometheusExporter;
        private readonly IRedisCacheService _redisCacheService;

        public WebHookService(IPrometheusExporterService prometheusExporter, IRedisCacheService redisCacheService)
        {
            _prometheusExporter = prometheusExporter;
            _redisCacheService = redisCacheService;
        }

        public async Task ExportMetricsForWebHook(WebHookModel webhook, CancellationToken stoppingToken)
        {
            LogMessage($"A webHook received. Card: {webhook?.Data?.Account}...");
            try
            {
                if (string.IsNullOrWhiteSpace(webhook?.Data?.Account) || webhook.Data?.StatementItem == null)
                {
                    return;
                }

                var cacheRecord = await _redisCacheService.GetRecordAsync(webhook.Data.Account, stoppingToken);

                var accountInfo = JsonConvert.DeserializeObject<AccountInfoModel>(cacheRecord);
                if (accountInfo == null)
                {
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

        private static void LogMessage(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] {message}");
        }
    }
}