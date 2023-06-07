using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monobank.Client;
using Monobank.Client.Extensions;
using MonobankExporter.Application.BackgroundServices;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Options;
using MonobankExporter.Application.Services;

namespace MonobankExporter.Service.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddSingleton<ILookupsMemoryCacheService, LookupsMemoryCacheService>();
        return services;
    }

    internal static IServiceCollection AddMetricsOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("metrics").Get<MetricsOptions>() ?? new MetricsOptions();
        services.AddSingleton(options);
        services.AddSingleton<IMetricsExporterService, PrometheusExporterService>();
        return services;
    }

    internal static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<ExportUserInfoBackgroundService>();
        services.AddHostedService<ExportCurrenciesBackgroundService>();
        return services;
    }

    internal static IServiceCollection AddMonobankService(this IServiceCollection services)
    {
        services.AddScoped<IMonobankService, MonobankService>();
        return services;
    }

    internal static IServiceCollection AddMonobankExporterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("monobank-exporter").Get<MonobankExporterOptions>() ?? new MonobankExporterOptions();
        services.AddSingleton(options);
        return services;
    }

    internal static IServiceCollection AddBasicAuthOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("basic-auth").Get<BasicAuthOptions>() ?? new BasicAuthOptions();
        services.AddSingleton(options);
        return services;
    }

    internal static IServiceCollection AddMonobankClient(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection("monobank-api").Get<MonobankClientOptions>() ?? new MonobankClientOptions();
        services.AddMonobankClient(options);
        return services;
    }
}