using Microsoft.AspNetCore.Builder;
using MonobankExporter.API.Middleware;

namespace MonobankExporter.API.Extensions
{
    internal static class ApplicationBuilderExtensions
    {
        internal static IApplicationBuilder UseBasicAuth(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<BasicAuthMiddleware>();
            return builder;
        }
    }
}