using Microsoft.AspNetCore.Builder;
using MonobankExporter.Service.Middleware;

namespace MonobankExporter.Service.Extensions;

internal static class ApplicationBuilderExtensions
{
    internal static IApplicationBuilder UseBasicAuth(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<BasicAuthMiddleware>();
        return builder;
    }
}