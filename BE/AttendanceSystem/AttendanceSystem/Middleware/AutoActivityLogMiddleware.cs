using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AttendanceSystem.Middlewares
{
    public class AutoActivityLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AutoActivityLogMiddleware> _logger;

        public AutoActivityLogMiddleware(RequestDelegate next, ILogger<AutoActivityLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IActivityLogService logService)
        {
            // Bỏ qua các route không cần log
            if (context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/health"))
            {
                await _next(context);
                return;
            }

            var currentUserId = context.Session.GetInt32("UserId");

            if (currentUserId.HasValue)
            {
                var logDto = new CreateActivityLogDTO
                {
                    UserId = currentUserId.Value,
                    Action = $"{context.Request.Method}_{context.Request.Path}",
                    Description = $"Truy cập {context.Request.Path}",
                    IPAddress = context.Connection.RemoteIpAddress?.ToString(),
                    DeviceInfo = context.Request.Headers["User-Agent"].ToString()
                };

                try
                {
                    // Ghi log ngay trong context hiện tại — an toàn với DbContext
                    await logService.LogActivityAsync(logDto);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Không thể ghi log hoạt động");
                }
            }

            await _next(context);
        }
    }
}
