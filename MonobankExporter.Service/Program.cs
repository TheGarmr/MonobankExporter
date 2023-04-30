using Microsoft.AspNetCore.Builder;
using Serilog;
using System.Text;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MonobankExporter.Service.Extensions;
using Prometheus;
using Monobank.Client.Models;
using System.Threading;
using MonobankExporter.Application.Interfaces;

Console.OutputEncoding = Encoding.UTF8;
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
var version = builder.LogStartMessageWithVersion(Log.Logger);
Log.Logger = builder.AddLogger(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddCache();
builder.Services.AddMetricsOptions(builder.Configuration);
builder.Services.AddMonobankExporterOptions(builder.Configuration);
builder.Services.AddMonobankService();
builder.Services.AddMonobankClient(builder.Configuration);
builder.Services.AddBackgroundWorkers();
builder.Services.AddBasicAuthOptions(builder.Configuration);

var app = builder.Build();
app.MapGet("/", () => $"monobank-exporter {version}");
app.Map("/metrics", metricsEndpoint =>
{
    metricsEndpoint.UseBasicAuth();
    metricsEndpoint.UseMetricServer("");
});

app.MapGet("/webhook", () => Results.Ok());
app.MapPost("/webhook", (WebHook webhook, CancellationToken stoppingToken, IMonobankService monobankService) =>
    {
        monobankService.ExportMetricsOnWebHook(webhook, stoppingToken);
        Results.Ok();
    }
);
Metrics.SuppressDefaultMetrics();
app.Run();