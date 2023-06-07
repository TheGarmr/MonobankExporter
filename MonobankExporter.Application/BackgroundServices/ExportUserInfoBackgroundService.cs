using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Options;

namespace MonobankExporter.Application.BackgroundServices;

public class ExportUserInfoBackgroundService : BackgroundService
{
    private readonly MonobankExporterOptions _options;
    private readonly IMonobankService _monobankService;
    private readonly ILogger<ExportUserInfoBackgroundService> _logger;

    public ExportUserInfoBackgroundService(MonobankExporterOptions options,
        IServiceScopeFactory scopeFactory)
    {
        _options = options;
        var scope = scopeFactory.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<ExportUserInfoBackgroundService>>();
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

            var validClients = await _monobankService.SetupWebHookAndExportMetricsForUsersAsync(_options.WebhookUrl, _options.Clients, stoppingToken);
            if (!validClients.Any())
            {
                _logger.LogWarning("There are no valid clients to expose metrics.");
                return;
            }
            Thread.Sleep(TimeSpan.FromMinutes(_options.ClientsRefreshTimeInMinutes));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    await _monobankService.ExportBalanceMetricsForUsersAsync(validClients, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation($"Stopping {nameof(ExportUserInfoBackgroundService)}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{nameof(ExportUserInfoBackgroundService)} unexpectedly failed. ErrorType: {typeof(Exception)}. Error message: {ex.Message}");
                }

                Thread.Sleep(TimeSpan.FromMinutes(_options.ClientsRefreshTimeInMinutes));
            }
        }, stoppingToken);
    }
}