using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MonobankExporter.Service.Extensions
{
    internal static class WebHostExtensions
    {
        internal static IWebHostBuilder ConfigureAppConfigurations(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration((hostingContext, configBuilder) =>
            {
                var env = hostingContext.HostingEnvironment;

                configBuilder
                    .AddJsonFile("./appsettings.json")
                    .AddYamlFile("./monobank-exporter.yml", optional: true)
                    .AddYamlFile($"./monobank-exporter.{env.EnvironmentName}.yml", optional: true);
            });
        }
    }
}