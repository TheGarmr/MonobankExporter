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
    public class CurrenciesWorker : BackgroundService
    {
        private readonly MonobankExporterOptions _options;
        private readonly IServiceScopeFactory _scopeFactory;

        public CurrenciesWorker(MonobankExporterOptions options,
            IServiceScopeFactory scopeFactory)
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
                    Thread.Sleep(TimeSpan.FromMinutes(_options.CurrenciesRefreshTimeInMinutes));
                }
            }, stoppingToken);
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<BalanceWorker>>();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var monobankService = scope.ServiceProvider.GetRequiredService<IMonobankService>();
                await monobankService.ExportCurrenciesMetrics(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError($"Currencies export unexpectedly failed. Error message: {ex.Message}");
            }
        }
    }
}