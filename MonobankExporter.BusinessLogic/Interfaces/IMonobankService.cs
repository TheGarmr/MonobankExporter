using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monobank.Client.Models;
using MonobankExporter.BusinessLogic.Options;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMonobankService
    {
        Task SetupWebHookAndExportMetricsForUsersAsync(string webHookUrl, List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        Task ExportMetricsForUsersAsync(List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        Task ExportMetricsForCurrenciesAsync(CancellationToken stoppingToken);
        void ExportMetricsOnWebHook(WebHook webhook, CancellationToken stoppingToken);
        bool WebHookUrlIsValid(string webHookUrl);
    }
}