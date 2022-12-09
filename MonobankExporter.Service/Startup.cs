using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MonobankExporter.Service.Extensions;
using Prometheus;

namespace MonobankExporter.Service
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
            var logger = Serilog.Log.Logger = services.AddLogger(Configuration);
            var version = GetType().Assembly.GetName().Version;
            logger.Information($"Running monobank-exporter v{version.Major}.{version.Minor}.{version.Build}");
            services.AddControllers();
            services.AddCache();
            services.AddMetricsOptions(Configuration);
            services.AddMonobankExporterOptions(Configuration);
            services.AddMonobankService();
            services.AddMonobankClient(Configuration);
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
