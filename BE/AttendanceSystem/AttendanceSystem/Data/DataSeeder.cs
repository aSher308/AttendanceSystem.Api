using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;
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
            if (!await context.Users.AnyAsync(u => u.Email == "manhcuucon@gmail.com"))
            {
                var adminUser = new User
                {
                    FullName = "Administrator",
                    Email = "manhcuucon@gmail.com",
                    PhoneNumber = "0123456789",
                    PasswordHash = PasswordHasher.Hash("Admin@123"),
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

        public static async Task SeedShiftsAsync(AppDbContext context)
        {
            if (!await context.Shifts.AnyAsync())
            {
                context.Shifts.AddRange(
                    new Shift
                    {
                        Name = "Ca sáng",
                        StartTime = new TimeSpan(7, 0, 0),
                        EndTime = new TimeSpan(11, 30, 0),
                        IsActive = true,
                        Description = "Ca làm buổi sáng"
                    },
                    new Shift
                    {
                        Name = "Ca chiều",
                        StartTime = new TimeSpan(13, 0, 0),
                        EndTime = new TimeSpan(17, 30, 0),
                        IsActive = true,
                        Description = "Ca làm buổi chiều"
                    },
                    new Shift
                    {
                        Name = "Ca tối",
                        StartTime = new TimeSpan(18, 0, 0),
                        EndTime = new TimeSpan(22, 0, 0),
                        IsActive = false,
                        Description = "Ca làm buổi tối"
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
