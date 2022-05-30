using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MonobankExporter.API.Extensions
{
    internal static class WebHostExtensions
    {
        internal static IWebHostBuilder ConfigureAppConfigurations(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
            {
                var env = hostingContext.HostingEnvironment;

                configBuilder
                    .AddYamlFile("./monobank-exporter.yml", optional: true, reloadOnChange: true)
                    .AddYamlFile($"./monobank-exporter.{env.EnvironmentName}.yml", optional: true, reloadOnChange: true);
            });
        }
    }
}