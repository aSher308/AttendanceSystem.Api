using AttendanceSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.Attributes
{
    public class RequireAdminAttribute : Attribute, IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var services = httpContext.RequestServices;
            var db = services.GetRequiredService<AppDbContext>();

            var userId = httpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var isAdmin = await db.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "Admin");

            if (!isAdmin)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
