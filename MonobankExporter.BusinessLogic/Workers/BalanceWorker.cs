using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.BusinessLogic.Models;

namespace MonobankExporter.BusinessLogic.Workers
{
    public class BalanceWorker : BackgroundService
    {
        private readonly MonobankExporterOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;

        public BalanceWorker(MonobankExporterOptions options,
            IServiceScopeFactory scopeFactory)
        {
            _options = options;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                var webhookWillBeUsed = WebHookWillBeUsed();
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessAsync(webhookWillBeUsed, stoppingToken);
                    Thread.Sleep(TimeSpan.FromMinutes(_options.ClientsRefreshTimeInMinutes));
                }
            }, stoppingToken);
        }

        private async Task ProcessAsync(bool webhookWillBeUsed, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BalanceWorker>>();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var monobankService = scope.ServiceProvider.GetRequiredService<IMonobankService>();
                if (webhookWillBeUsed)
                {
                    logger.LogInformation("Webhook url is valid. Webhook and Redis will be used.");
                    await monobankService.SetupWebHookForUsers(_options.WebhookUrl, cancellationToken);
                }
                await monobankService.ExportUsersMetrics(webhookWillBeUsed, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"Balance export unexpectedly failed. Error message: {ex.Message}");
            }
        }

        private bool WebHookWillBeUsed()
        {
            using var scope = _scopeFactory.CreateScope();
            var monobankService = scope.ServiceProvider.GetRequiredService<IMonobankService>();
            return monobankService.WebHookUrlIsValid(_options.WebhookUrl);
        }
    }
}