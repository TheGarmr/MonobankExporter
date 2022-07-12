using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monobank.Client.Models;
using MonobankExporter.Application.Options;

namespace MonobankExporter.Application.Interfaces
{
    public interface IMonobankService
    {
        Task<List<ClientInfoOptions>> SetupWebHookAndExportMetricsForUsersAsync(string webHookUrl,
            List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        Task ExportBalanceMetricsForUsersAsync(List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        Task ExportMetricsForCurrenciesAsync(CancellationToken stoppingToken);
        void ExportMetricsOnWebHook(WebHook webhook, CancellationToken stoppingToken);
        bool WebHookUrlIsValid(string webHookUrl);
    }
}