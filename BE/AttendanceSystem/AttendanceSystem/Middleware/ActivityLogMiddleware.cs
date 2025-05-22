using AttendanceSystem.Data;
using AttendanceSystem.Models;

namespace AttendanceSystem.Middleware
{
    public class ActivityLogMiddleware
    {
        private readonly RequestDelegate _next;

        public ActivityLogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            var userIdStr = context.User?.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                var action = $"{context.Request.Method} {context.Request.Path}";
                var deviceInfo = context.Request.Headers["User-Agent"].ToString();
                var ip = context.Connection.RemoteIpAddress?.ToString();

                db.ActivityLogs.Add(new ActivityLog
                {
                    UserId = userId,
                    Action = action,
                    Description = "", // Nếu cần có thể gắn thêm
                    Timestamp = VietnamTimeHelper.Now,
                    IPAddress = ip,
                    DeviceInfo = deviceInfo
                });

                await db.SaveChangesAsync();
            }

            await _next(context);
        }
    }

}
