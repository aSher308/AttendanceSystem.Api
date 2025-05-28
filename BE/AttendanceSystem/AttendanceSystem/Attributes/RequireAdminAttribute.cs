using AttendanceSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics; // Thêm dòng này

namespace AttendanceSystem.Attributes
{
    public class RequireRoleAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var httpContext = context.HttpContext;
            var services = httpContext.RequestServices;
            var db = services.GetRequiredService<AppDbContext>();

            var userId = httpContext.Session.GetInt32("UserId");
            Debug.WriteLine($"[DEBUG] UserId trong session: {userId}"); // Log userId

            if (userId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRoles = await db.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            Debug.WriteLine($"[DEBUG] UserId: {userId}, Roles: {string.Join(",", userRoles)}"); // Log roles

            if (!_roles.Any(role => userRoles.Contains(role)))
            {
                context.Result = new ForbidResult();
            }
        }
    }

    public class RequireAdminAttribute : RequireRoleAttribute
    {
        public RequireAdminAttribute() : base("Admin") { }
    }

    public class RequireUserAttribute : RequireRoleAttribute
    {
        public RequireUserAttribute() : base("User") { }
    }
}