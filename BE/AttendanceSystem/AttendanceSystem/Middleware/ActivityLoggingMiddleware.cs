// Middleware/ActivityLogApiMiddleware.cs
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AttendanceSystem.Middleware
{
    public class ActivityLogApiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ActivityLogApiMiddleware> _logger;
        private readonly string[] _excludedPaths = { "/swagger", "/health", "/favicon.ico" };

        public ActivityLogApiMiddleware(
            RequestDelegate next,
            ILogger<ActivityLogApiMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IActivityLogService logService)
        {
            if (_excludedPaths.Any(path => context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            try
            {
                await _next(context);

                if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
                {
                    await LogRequestAsync(context, logService);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "API Request Error: {Path}", context.Request.Path);
                throw;
            }
        }

        private async Task LogRequestAsync(HttpContext context, IActivityLogService logService)
        {
            var userIdClaim = context.User.FindFirst("id") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                await logService.LogActivityAsync(
                    userId,
                    $"{context.Request.Method} {context.Request.Path}",
                    $"Status: {context.Response.StatusCode}",
                    context.Connection.RemoteIpAddress?.ToString(),
                    context.Request.Headers["User-Agent"]);
            }
        }
    }
}