using AttendanceSystem.DTOs;
using AttendanceSystem.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace AttendanceSystem.Middlewares
{
    public class AutoActivityLogMiddleware
    {
        private readonly RequestDelegate _next;

        public AutoActivityLogMiddleware(RequestDelegate next)
        {
            _next = next;
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

            // Lấy UserId từ session
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

                // Ghi log không đồng bộ (không chờ kết quả)
                _ = logService.LogActivityAsync(logDto).ConfigureAwait(false);
            }

            await _next(context);
        }
    }
}