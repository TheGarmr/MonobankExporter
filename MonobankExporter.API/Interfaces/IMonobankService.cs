using System.Threading;
using System.Threading.Tasks;

namespace MonobankExporter.API.Interfaces
{
    public interface IMonobankService
    {
        Task ExportUsersMetrics(bool webhookWillBeUsed, CancellationToken stoppingToken);
        Task<bool> SetupWebHookForUsers(CancellationToken stoppingToken);
        Task ExportCurrenciesMetrics(CancellationToken stoppingToken);
    }
}