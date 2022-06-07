using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonobankExporter.BusinessLogic.Interfaces;
using MonobankExporter.BusinessLogic.Models;
using MonobankExporter.BusinessLogic.Services;
using MonobankExporter.BusinessLogic.Workers;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

namespace MonobankExporter.API.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IRedisCacheService, RedisCacheService>();
            var redisOptions = configuration.GetSection("redis").Get<RedisOptions>();
            if (redisOptions != null && !string.IsNullOrWhiteSpace(redisOptions.Host) && !string.IsNullOrWhiteSpace(redisOptions.Port))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.InstanceName = "MonobankExporter";
                    options.ConfigurationOptions = new ConfigurationOptions
                    {
                        EndPoints = { redisOptions.Host, redisOptions.Port },
                        AbortOnConnectFail = false
                    };
                });
            }
            else
            {
                services.Add(ServiceDescriptor.Singleton<IDistributedCache, DistributedCacheMock>());
            }

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