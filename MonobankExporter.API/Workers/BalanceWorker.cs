using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MonobankExporter.API.Interfaces;
using MonobankExporter.API.Models;

namespace MonobankExporter.API.Workers
{
    public class BalanceWorker : BackgroundService
    {
        private readonly IMonobankService _monobankService;
        private readonly MonobankExporterOptions _options;

        public BalanceWorker(IMonobankService monobankService, MonobankExporterOptions options)
        {
            _monobankService = monobankService;
            _options = options;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                var webhookWillBeUsed = await _monobankService.SetupWebHookForUsers(stoppingToken);
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _monobankService.ExportUsersMetrics(webhookWillBeUsed, stoppingToken);
                    Thread.Sleep(TimeSpan.FromMinutes(_options.ClientsRefreshTimeInMinutes));
                }
            }, stoppingToken);
        }
    }
}