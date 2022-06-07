using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonobankExporter.API.Extensions;
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
            var logger = Serilog.Log.Logger = services.AddLogger();
            logger.Information($"running monobank-exporter. version {GetType().Assembly.GetName().Version}");
            services.AddControllers();
            services.AddRedisCache(Configuration);
            services.AddMetricsExporters();
            services.AddMonobankExporterOptions(Configuration);
            services.AddMonobankService();
            services.AddBackgroundWorkers();
            services.AddBasicAuthOptions(Configuration);
        }
        
        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.Map("/metrics", metricsEndpoint =>
            {
                metricsEndpoint.UseBasicAuth();
                metricsEndpoint.UseMetricServer("");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Metrics.SuppressDefaultMetrics();
        }
    }
}
