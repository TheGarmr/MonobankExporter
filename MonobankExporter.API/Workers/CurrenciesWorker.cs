using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using MonobankExporter.API.Interfaces;
using MonobankExporter.API.Models;

namespace MonobankExporter.API.Workers
{
    public class CurrenciesWorker : BackgroundService
    {
        private readonly IMonobankService _monobankService;
        private readonly MonobankExporterOptions _options;

        public CurrenciesWorker(IMonobankService monobankService, MonobankExporterOptions options)
        {
            _monobankService = monobankService;
            _options = options;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _monobankService.ExportCurrenciesMetrics(stoppingToken);
                    Thread.Sleep(TimeSpan.FromMinutes(_options.CurrenciesRefreshTimeInMinutes));
                }
            }, stoppingToken);
        }
    }
}