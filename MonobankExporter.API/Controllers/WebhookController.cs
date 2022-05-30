using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Monobank.Core.Models;
using MonobankExporter.BusinessLogic.Interfaces;

namespace MonobankExporter.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IMonobankService _monobankService;

        public WebhookController(IMonobankService webHookService)
        {
            _monobankService = webHookService;
        }

        [HttpPost]
        public async Task<ActionResult> Webhook([FromBody] WebHookModel webhook, CancellationToken stoppingToken)
        {
            await _monobankService.ExportMetricsForWebHook(webhook, stoppingToken);
            return Ok();
        }
    }
}