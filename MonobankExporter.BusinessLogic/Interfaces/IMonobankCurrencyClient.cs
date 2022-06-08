using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonobankExporter.Domain.Models.Client;

namespace MonobankExporter.BusinessLogic.Interfaces
{
    public interface IMonobankCurrencyClient
    {
        Task<ICollection<CurrencyInfo>> GetCurrenciesAsync(CancellationToken stoppingToken);
    }
}