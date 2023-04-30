using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MonobankExporter.Service.Extensions;

public static class WebApplicationBuilderExtensions
{
    internal static ILogger AddLogger(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        return new LoggerConfiguration()
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }

    internal static AppVersion LogStartMessageWithVersion(this WebApplicationBuilder builder, ILogger logger)
    {
        var version = new AppVersion();
        logger.Information("----------------------------------------------------");
        logger.Information($"Running monobank-exporter {version}");
        return version;
    }
}