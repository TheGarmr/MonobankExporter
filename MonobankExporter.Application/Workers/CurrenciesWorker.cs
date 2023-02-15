using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Options;

namespace MonobankExporter.Application.Workers;

public class CurrenciesWorker : BackgroundService
{
    private readonly MonobankExporterOptions _options;
    private readonly IMonobankService _monobankService;
    private readonly ILogger<CurrenciesWorker> _logger;

    public CurrenciesWorker(MonobankExporterOptions options,
        IServiceScopeFactory scopeFactory)
    {
        _options = options;
        var scope = scopeFactory.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<CurrenciesWorker>>();
        _monobankService = scope.ServiceProvider.GetRequiredService<IMonobankService>();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    await _monobankService.ExportMetricsForCurrenciesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Stopping currencies metrics export");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Currencies export unexpectedly failed. Error message: {ex.Message}");
                }

                Thread.Sleep(TimeSpan.FromMinutes(_options.CurrenciesRefreshTimeInMinutes));
            }
        }, stoppingToken);
    }
}