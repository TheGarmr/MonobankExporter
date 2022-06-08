using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonobankExporter.BusinessLogic.Models;
using MonobankExporter.Client.Models;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMonobankService
    {
        Task ExportMetricsForUsersAsync(bool storeToCache, List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        Task ExportMetricsForCurrenciesAsync(CancellationToken stoppingToken);
        void ExportMetricsOnWebHook(WebHookModel webhook, CancellationToken stoppingToken);
        Task SetupWebHookForUsersAsync(string webHookUrl, List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        bool WebHookUrlIsValid(string webHookUrl);
    }
}