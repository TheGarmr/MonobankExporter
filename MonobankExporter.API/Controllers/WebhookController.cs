using System.Threading;
using Microsoft.AspNetCore.Mvc;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.Domain.Models;
using MonobankExporter.Domain.Models.Client;

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
        public ActionResult Webhook([FromBody] WebHook webhook, CancellationToken stoppingToken)
        {
            _monobankService.ExportMetricsOnWebHook(webhook, stoppingToken);
            return Ok();
        }
    }
}