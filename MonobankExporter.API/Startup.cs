using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MonobankExporter.API.Extensions;
using MonobankExporter.API.Middleware;
using Prometheus;

namespace MonobankExporter.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine($"[{DateTime.Now}] running monobank-exporter. version 1.1");
            services.AddControllers();
            services.AddRedisCache(Configuration);
            services.AddPrometheusExporter();
            services.AddMonobankExporterOptions(Configuration);
            services.AddMonobankService();
            services.AddBackgroundWorkers();
            services.AddBasicAuthOptions(Configuration);
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.Map("/metrics", metricsApp =>
            {
                metricsApp.UseMiddleware<BasicAuthMiddleware>();
                metricsApp.UseMetricServer("");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Metrics.SuppressDefaultMetrics();
        }
    }
}
