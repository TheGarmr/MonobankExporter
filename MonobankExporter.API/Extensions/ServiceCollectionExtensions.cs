using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Monobank.Client;
using Monobank.Client.Extensions;
using MonobankExporter.Application.Interfaces;
using MonobankExporter.Application.Options;
using MonobankExporter.Application.Services;
using MonobankExporter.Application.Workers;
using Serilog;
using Serilog.Events;

namespace MonobankExporter.API.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            services.AddSingleton<ILookupsMemoryCacheService, LookupsMemoryCacheService>();
            return services;
        }

        internal static IServiceCollection AddMetricsExporters(this IServiceCollection services)
        {
            var options = new MetricsExporterOptions();
            services.AddSingleton(options);
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

        internal static ILogger AddLogger(this IServiceCollection services, IConfiguration configuration)
        {
            return new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}