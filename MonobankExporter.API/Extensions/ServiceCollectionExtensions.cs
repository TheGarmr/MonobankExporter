﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonobankExporter.API.Interfaces;
using MonobankExporter.API.Models;
using MonobankExporter.API.Services;
using MonobankExporter.API.Workers;
using StackExchange.Redis;

namespace MonobankExporter.API.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IRedisCacheService, RedisCacheService>();
            var redisOptions = configuration.GetSection("redis").Get<RedisOptions>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.InstanceName = "MonobankExporter";
                options.ConfigurationOptions = new ConfigurationOptions
                {
                    EndPoints = { redisOptions.Host, redisOptions.Port },
                    AbortOnConnectFail = false
                };
            });

            return services;
        }

        internal static IServiceCollection AddPrometheusExporter(this IServiceCollection services)
        {
            services.AddSingleton<IPrometheusExporterService, PrometheusExporterService>();
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
            services.AddSingleton<IMonobankService, MonobankService>();
            services.AddSingleton<IWebHookService, WebHookService>();
            return services;
        }

        internal static IServiceCollection AddMonobankExporterOptions(this IServiceCollection services, IConfiguration configuration)
        {
            var options = configuration.GetSection("monobank-exporter").Get<MonobankExporterOptions>();
            services.AddSingleton(options);
            return services;
        }
    }
}