using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Options;

namespace MonobankExporter.Application.Workers;

public class UserInfoExportWorker : BackgroundService
{
    private readonly MonobankExporterOptions _options;
    private readonly IMonobankService _monobankService;
    private readonly ILogger<UserInfoExportWorker> _logger;

    public UserInfoExportWorker(MonobankExporterOptions options,
        IServiceScopeFactory scopeFactory)
    {
        _options = options;
        var scope = scopeFactory.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<UserInfoExportWorker>>();
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
                    _logger.LogInformation($"Stopping {nameof(UserInfoExportWorker)}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"{nameof(UserInfoExportWorker)} unexpectedly failed. Error message: {ex.Message}");
                }

                Thread.Sleep(TimeSpan.FromMinutes(_options.ClientsRefreshTimeInMinutes));
            }
        }, stoppingToken);
    }
}