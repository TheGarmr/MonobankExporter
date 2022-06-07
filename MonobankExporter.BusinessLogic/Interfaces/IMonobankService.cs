using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonobankExporter.BusinessLogic.Models;
using MonobankExporter.Client.Models;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMonobankService
    {
        Task ExportUsersMetricsAsync(bool storeToCache, List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        Task SetupWebHookForUsersAsync(string webHookUrl, List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        bool WebHookUrlIsValid(string webHookUrl);
        Task ExportCurrenciesMetricsAsync(CancellationToken stoppingToken);
        void ExportMetricsForWebHook(WebHookModel webhook, CancellationToken stoppingToken);
    }
}