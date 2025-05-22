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
                    CreatedAt = VietnamTimeHelper.Now,
                    UpdatedAt = VietnamTimeHelper.Now
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

        public static async Task SeedLocationsAsync(AppDbContext context)
        {
            if (!context.Locations.Any())
            {
                var locations = new List<Location>
                {
                    new Location
                    {
                        Name = "Khu E",
                        Latitude = 10.8550348,
                        Longitude = 106.7847038,
                        RadiusInMeters = 100,
                        IsDefault = true
                    },
                    new Location
                    {
                        Name = "Khu R",
                        Latitude = 10.8408075,
                        Longitude = 106.8088987,
                        RadiusInMeters = 100,
                        IsDefault = false
                    },
                    new Location
                    {
                        Name = "Khu AB",
                        Latitude = 10.8095728,
                        Longitude = 106.7149885,
                        RadiusInMeters = 100,
                        IsDefault = false
                    },
                    new Location
                    {
                        Name = "Nhà tôi",
                        Latitude = 10.884845,
                        Longitude = 106.784789,
                        RadiusInMeters = 100,
                        IsDefault = false
                    }
                };

                context.Locations.AddRange(locations);
                await context.SaveChangesAsync();
            }
        }
    }
}
