using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MonobankExporter.Domain.Options;

namespace MonobankExporter.API.Middleware
{
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly BasicAuthOptions _options;

        public BasicAuthMiddleware(RequestDelegate next, BasicAuthOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (AuthIsEnabled() && !UserIsAuthorized(context))
            {
                context.Response.Headers["WWW-Authenticate"] = "Basic";

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }
            
            await _next.Invoke(context);
        }

        private bool UserIsAuthorized(HttpContext context)
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                var encodedUsernamePassword = authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim();

                var decodedUsernamePassword = Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];

                return UsernameAndPasswordAreValid(username, password);
            }

            return false;
        }

        private bool UsernameAndPasswordAreValid(string username, string password)
        {
            return username.Equals(_options.Username) && password.Equals(_options.Password);
        }

        private bool AuthIsEnabled()
        {
            return !string.IsNullOrWhiteSpace(_options.Username) && !string.IsNullOrWhiteSpace(_options.Password);
        }
    }
}