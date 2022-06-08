using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MonobankExporter.Client.Models;

namespace MonobankExporter.Client.Services
{
    public interface IMonobankServiceClient
    {
        Task<UserInfo> GetClientInfoAsync(string token, CancellationToken cancellationToken);
        Task<ICollection<Statement>> GetStatementsAsync(DateTime from, DateTime to, string account = "0");
        Task<bool> SetWebhookAsync(string url, string token, CancellationToken stoppingToken);
    }
}