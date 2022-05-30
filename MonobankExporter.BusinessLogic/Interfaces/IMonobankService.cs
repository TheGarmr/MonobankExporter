using System.Threading;
using System.Threading.Tasks;
using Monobank.Core.Models;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMonobankService
    {
        Task ExportUsersMetrics(bool webhookWillBeUsed, CancellationToken stoppingToken);
        Task SetupWebHookForUsers(string webHookUrl, CancellationToken stoppingToken);
        bool WebHookUrlIsValid(string webHookUrl);
        Task ExportCurrenciesMetrics(CancellationToken stoppingToken);
        Task ExportMetricsForWebHook(WebHookModel webhook, CancellationToken stoppingToken);
    }
}