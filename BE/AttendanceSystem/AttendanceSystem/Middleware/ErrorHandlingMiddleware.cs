using System.Text.Json;
using AttendanceSystem.Exceptions;

namespace AttendanceSystem.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 400;

            var response = new { error = exception is AppException ? exception.Message : "Đã xảy ra lỗi hệ thống" };
            var json = JsonSerializer.Serialize(response);

            return context.Response.WriteAsync(json);
        }
    }

    // Extension để gọi middleware gọn trong Program.cs
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}
