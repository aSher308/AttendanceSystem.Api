using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Models;
using AttendanceSystem.Helpers;
namespace AttendanceSystem.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAsync(AppDbContext context)
        {
            if (!await context.Roles.AnyAsync())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "User" }
                );
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedAdminUserAsync(AppDbContext context)
        {
            // Kiểm tra nếu chưa có user admin
            if (!await context.Users.AnyAsync(u => u.Email == "admin@example.com"))
            {
                var adminUser = new User
                {
                    FullName = "Administrator",
                    Email = "admin@example.com",
                    PhoneNumber = "0123456789",
                    PasswordHash = PasswordHasher.Hash("Admin@123"), // 👈 đổi mật khẩu sau
                    LeaveBalance = 30,
                    IsEmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    context.UserRoles.Add(new UserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}