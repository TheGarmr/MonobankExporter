using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonobankExporter.Client.Models;

namespace MonobankExporter.Client.Services
{
    public interface IMonobankCurrencyClient
    {
        Task<ICollection<CurrencyInfo>> GetCurrenciesAsync(CancellationToken stoppingToken);
    }
}