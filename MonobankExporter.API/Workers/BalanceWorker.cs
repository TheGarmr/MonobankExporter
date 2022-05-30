using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonobankExporter.API.Interfaces;
using MonobankExporter.API.Models;

namespace MonobankExporter.API.Workers
{
    public class BalanceWorker : BackgroundService
    {
        private readonly MonobankExporterOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;

        public BalanceWorker(MonobankExporterOptions options, IServiceScopeFactory scopeFactory)
        {
            _options = options;
            _scopeFactory = scopeFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ProcessAsync(stoppingToken);
                    Thread.Sleep(TimeSpan.FromMinutes(_options.ClientsRefreshTimeInMinutes));
                }
            }, stoppingToken);
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var monobankService = scope.ServiceProvider.GetRequiredService<IMonobankService>();
                var webhookWillBeUsed = monobankService.WebHookUrlIsValid(_options.WebhookUrl);
                if (webhookWillBeUsed)
                {
                    await monobankService.SetupWebHookForUsers(_options.WebhookUrl, cancellationToken);
                }
                await monobankService.ExportUsersMetrics(webhookWillBeUsed, cancellationToken);
            }
            catch
            {
                Console.WriteLine($"[{DateTime.Now}] ");
            }
        }
    }
}