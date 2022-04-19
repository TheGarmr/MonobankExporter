using System.Threading;
using System.Threading.Tasks;
using Monobank.Core.Models;

namespace MonobankExporter.API.Interfaces
{
    public interface IWebHookService
    {
        Task ExportMetricsForWebHook(WebHookModel webhook, CancellationToken stoppingToken);
    }
}