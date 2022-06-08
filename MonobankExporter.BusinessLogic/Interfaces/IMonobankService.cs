using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonobankExporter.Domain.Models.Client;
using MonobankExporter.Domain.Options;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMonobankService
    {
        Task ExportMetricsForUsersAsync(bool storeToCache, List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        Task ExportMetricsForCurrenciesAsync(CancellationToken stoppingToken);
        void ExportMetricsOnWebHook(WebHook webhook, CancellationToken stoppingToken);
        Task SetupWebHookForUsersAsync(string webHookUrl, List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        bool WebHookUrlIsValid(string webHookUrl);
    }
}