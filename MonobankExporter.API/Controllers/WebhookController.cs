using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Monobank.Core.Models;
using MonobankExporter.BusinessLogic.Interfaces;

namespace MonobankExporter.API.Controllers
{
    [ApiController]
    [Route("webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly IMonobankService _monobankService;

        public WebhookController(IMonobankService webHookService)
        {
            _monobankService = webHookService;
        }

        [HttpGet]
        public IActionResult Webhook()
        {
            return Ok();
        }

        [HttpPost]
        public ActionResult Webhook([FromBody] WebHookModel webhook, CancellationToken stoppingToken)
        {
            _monobankService.ExportMetricsForWebHook(webhook, stoppingToken);
            return Ok();
        }
    }
}