using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace MonobankExporter.API.Extensions
{
    internal static class WebHostExtensions
    {
        internal static IWebHostBuilder ConfigureAppConfigurations(this IWebHostBuilder builder)
        {
            return builder.ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.AddYamlFile("./monobank-exporter.yml", optional: false);
            });
        }
    }
}