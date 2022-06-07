using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Monobank.Core.Models;
using MonobankExporter.BusinessLogic.Models;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMonobankService
    {
        Task ExportUsersMetrics(bool storeToCache, CancellationToken stoppingToken);
        Task SetupWebHookForUsers(string webHookUrl, List<ClientInfoOptions> clients, CancellationToken stoppingToken);
        bool WebHookUrlIsValid(string webHookUrl);
        Task ExportCurrenciesMetrics(CancellationToken stoppingToken);
        void ExportMetricsForWebHook(WebHookModel webhook, CancellationToken stoppingToken);
    }
}