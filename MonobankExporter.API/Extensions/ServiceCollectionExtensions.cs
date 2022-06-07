using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.BusinessLogic.Models;
using MonobankExporter.BusinessLogic.Services;
using MonobankExporter.BusinessLogic.Workers;
using MonobankExporter.Client;
using Serilog;
using Serilog.Events;

namespace MonobankExporter.API.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            services.AddSingleton<ILookupsMemoryCache, LookupsMemoryCache>();
            return services;
        }

        internal static IServiceCollection AddMetricsExporters(this IServiceCollection services)
        {
            services.AddSingleton<IMetricsExporterService, PrometheusExporterService>();
            return services;
        }

        internal static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
        {
            services.AddHostedService<BalanceWorker>();
            services.AddHostedService<CurrenciesWorker>();
            return services;
        }

        internal static IServiceCollection AddMonobankService(this IServiceCollection services)
        {
            services.AddScoped<IMonobankService, MonobankService>();
            services.AddScoped<IMonoClient, MonoClient>();
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

        internal static ILogger AddLogger(this IServiceCollection services)
        {
            return new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("/var/log/monobank-exporter.log")
                .CreateLogger();
        }
    }
}