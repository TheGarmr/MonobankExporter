using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Monobank.Core.Models;
using MonobankExporter.API.Interfaces;

namespace MonobankExporter.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly IWebHookService _webHookService;

        public WebhookController(IWebHookService webHookService)
        {
            _webHookService = webHookService;
        }

        [HttpPost]
        public async Task<ActionResult> Webhook([FromBody] WebHookModel webhook, CancellationToken stoppingToken)
        {
            await _webHookService.ExportMetricsForWebHook(webhook, stoppingToken);
            return Ok();
        }
    }
}