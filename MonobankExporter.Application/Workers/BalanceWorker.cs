using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Options;

namespace MonobankExporter.Application.Workers
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
                if (!_options.Clients.Any())
                {
                    _logger.LogWarning("List of clients is empty. Metrics could not be exported.");
                    return;
                }

                await _monobankService.SetupWebHookAndExportMetricsForUsersAsync(_options.WebhookUrl, _options.Clients, stoppingToken);
                Thread.Sleep(TimeSpan.FromMinutes(_options.ClientsRefreshTimeInMinutes));

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        stoppingToken.ThrowIfCancellationRequested();
                        await _monobankService.ExportBalanceMetricsForUsersAsync(_options.Clients, stoppingToken);
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
    }
}