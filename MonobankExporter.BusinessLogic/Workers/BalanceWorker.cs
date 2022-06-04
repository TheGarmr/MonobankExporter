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
        private readonly IMonobankService _monobankService;
        private readonly ILogger<BalanceWorker> _logger;

        public BalanceWorker(MonobankExporterOptions options,
            IServiceScopeFactory scopeFactory)
        {
            _options = options;
            var scope = scopeFactory.CreateScope();
            _logger = scope.ServiceProvider.GetRequiredService<ILogger<BalanceWorker>>();
            _monobankService = scope.ServiceProvider.GetRequiredService<IMonobankService>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                var webhookWillBeUsed = await WebHookWillBeUsed(stoppingToken);
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        stoppingToken.ThrowIfCancellationRequested();
                        await _monobankService.ExportUsersMetrics(webhookWillBeUsed, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Stopping balance metrics export");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Balance export unexpectedly failed. Error message: {ex.Message}");
                    }

                    Thread.Sleep(TimeSpan.FromMinutes(_options.ClientsRefreshTimeInMinutes));
                }
            }, stoppingToken);
        }

        private async Task<bool> WebHookWillBeUsed(CancellationToken cancellationToken)
        {
            var webhookWillBeUsed = _monobankService.WebHookUrlIsValid(_options.WebhookUrl);
            if (webhookWillBeUsed)
            {
                _logger.LogInformation("Webhook url is valid. Webhook and Redis will be used.");
                await _monobankService.SetupWebHookForUsers(_options.WebhookUrl, cancellationToken);
            }
            return webhookWillBeUsed;
        }
    }
}