using AttendanceSystem.Models;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Helpers;

namespace AttendanceSystem.Data
{
    public static class DataSeeder
    {
        // =========================================================================
        // PHẦN 1: CODE CŨ CỦA BẠN (GIỮ NGUYÊN KHÔNG SỬA)
        // =========================================================================

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

        public static async Task SeedDepartmentsAsync(AppDbContext context)
        {
            if (!await context.Departments.AnyAsync())
            {
                var departments = new List<Department>
                {
                    new Department { Name = "Ban Giám đốc", Description = "Phòng ban quản lý cấp cao nhất", CreatedAt = VietnamTimeHelper.Now, UpdatedAt = VietnamTimeHelper.Now },
                    new Department { Name = "Phòng Nhân sự", Description = "Quản lý nhân sự, tuyển dụng, đào tạo", CreatedAt = VietnamTimeHelper.Now, UpdatedAt = VietnamTimeHelper.Now },
                    new Department { Name = "Phòng Kế toán", Description = "Quản lý tài chính, kế toán doanh nghiệp", CreatedAt = VietnamTimeHelper.Now, UpdatedAt = VietnamTimeHelper.Now },
                    new Department { Name = "Phòng Kỹ thuật", Description = "Phát triển và bảo trì hệ thống công nghệ", CreatedAt = VietnamTimeHelper.Now, UpdatedAt = VietnamTimeHelper.Now },
                    new Department { Name = "Phòng Kinh doanh", Description = "Phụ trách hoạt động kinh doanh và sales", CreatedAt = VietnamTimeHelper.Now, UpdatedAt = VietnamTimeHelper.Now },
                    new Department { Name = "Phòng Marketing", Description = "Quảng bá thương hiệu và sản phẩm", CreatedAt = VietnamTimeHelper.Now, UpdatedAt = VietnamTimeHelper.Now }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedAdminUserAsync(AppDbContext context)
        {
            if (!await context.Users.AnyAsync(u => u.Email == "manhcuucon@gmail.com"))
            {
                var adminDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Ban Giám đốc");

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
                    UpdatedAt = VietnamTimeHelper.Now,
                    DepartmentId = adminDepartment?.Id
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();

                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
                if (adminRole != null)
                {
                    context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id, AssignedAt = VietnamTimeHelper.Now });
                    await context.SaveChangesAsync();
                }
            }
        }

        public static async Task SeedShiftsAsync(AppDbContext context)
        {
            if (!await context.Shifts.AnyAsync())
            {
                context.Shifts.AddRange(
                    new Shift { Name = "Ca sáng", StartTime = new TimeSpan(7, 0, 0), EndTime = new TimeSpan(11, 30, 0), IsActive = true, Description = "Ca làm buổi sáng" },
                    new Shift { Name = "Ca chiều", StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(17, 30, 0), IsActive = true, Description = "Ca làm buổi chiều" },
                    new Shift { Name = "Ca tối", StartTime = new TimeSpan(18, 0, 0), EndTime = new TimeSpan(22, 0, 0), IsActive = false, Description = "Ca làm buổi tối" }
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
                    new Location { Name = "Khu E", Latitude = 10.8550348, Longitude = 106.7847038, RadiusInMeters = 100, IsDefault = true },
                    new Location { Name = "Khu R", Latitude = 10.8408075, Longitude = 106.8088987, RadiusInMeters = 100, IsDefault = false },
                    new Location { Name = "Khu AB", Latitude = 10.8095728, Longitude = 106.7149885, RadiusInMeters = 100, IsDefault = false },
                    new Location { Name = "Nhà tôi", Latitude = 10.884845, Longitude = 106.784789, RadiusInMeters = 100, IsDefault = false }
                };
                context.Locations.AddRange(locations);
                await context.SaveChangesAsync();
            }
        }

        // =========================================================================
        // PHẦN 2: CÁC HÀM NÂNG CẤP (THÊM DỮ LIỆU NHÂN VIÊN & CHẤM CÔNG)
        // =========================================================================

        // 1. Hàm tạo thêm 10 nhân viên (Không đụng đến Admin cũ)
        public static async Task SeedEmployeesAsync(AppDbContext context)
        {
            // Nếu đã có hơn 1 user (tức là đã có nhân viên ngoài admin), thì không seed thêm nữa
            if (await context.Users.CountAsync() > 1) return;

            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            var depts = await context.Departments.ToListAsync();
            var passwordHash = PasswordHasher.Hash("123456"); // Mật khẩu chung
            var now = VietnamTimeHelper.Now;

            var employees = new List<User>
            {
                new User { FullName = "Nguyễn Văn An", Email = "a@test.com", PhoneNumber = "0901000001", DepartmentId = depts.FirstOrDefault(d => d.Name == "Phòng Kỹ thuật")?.Id },
                new User { FullName = "Trần Thị Bích", Email = "b@test.com", PhoneNumber = "0901000002", DepartmentId = depts.FirstOrDefault(d => d.Name == "Phòng Nhân sự")?.Id },
                new User { FullName = "Lê Hoàng Cường", Email = "c@test.com", PhoneNumber = "0901000003", DepartmentId = depts.FirstOrDefault(d => d.Name == "Phòng Kinh doanh")?.Id },
                new User { FullName = "Phạm Minh Dung", Email = "dung.pham@test.com", PhoneNumber = "0901000004", DepartmentId = depts.FirstOrDefault(d => d.Name == "Phòng Kế toán")?.Id },
                new User { FullName = "Võ Tấn Em", Email = "e@test.com", PhoneNumber = "0901000005", DepartmentId = depts.FirstOrDefault(d => d.Name == "Phòng Kỹ thuật")?.Id },
                new User { FullName = "Đặng Thùy F", Email = "f@test.com", PhoneNumber = "0901000006", DepartmentId = depts.FirstOrDefault(d => d.Name == "Phòng Marketing")?.Id },
                new User { FullName = "Ngô Văn G", Email = "g@test.com", PhoneNumber = "0901000007", DepartmentId = depts.FirstOrDefault(d => d.Name == "Phòng Kinh doanh")?.Id },
                new User { FullName = "Hoàng Thị H", Email = "h@test.com", PhoneNumber = "0901000008", DepartmentId = depts.FirstOrDefault(d => d.Name == "Phòng Nhân sự")?.Id }
            };

            foreach (var emp in employees)
            {
                emp.PasswordHash = passwordHash;
                emp.LeaveBalance = 12;
                emp.IsActive = true;
                emp.IsEmailConfirmed = true;
                emp.CreatedAt = now;
                emp.UpdatedAt = now;
                context.Users.Add(emp);
            }
            await context.SaveChangesAsync();

            // Gán Role User cho nhân viên
            if (userRole != null)
            {
                foreach (var emp in employees)
                {
                    context.UserRoles.Add(new UserRole { UserId = emp.Id, RoleId = userRole.Id, AssignedAt = now });
                }
                await context.SaveChangesAsync();
            }
        }

        // 2. Hàm tạo lịch làm việc cho tháng hiện tại (Sử dụng Shift Ca sáng/Ca chiều của bạn)
        public static async Task SeedWorkSchedulesAsync(AppDbContext context)
        {
            if (await context.WorkSchedules.AnyAsync()) return;

            var users = await context.Users.Where(u => u.Email != "manhcuucon@gmail.com").ToListAsync(); // Bỏ qua Admin

            // Tìm đúng ca của bạn
            var shiftSang = await context.Shifts.FirstOrDefaultAsync(s => s.Name == "Ca sáng");
            var shiftChieu = await context.Shifts.FirstOrDefaultAsync(s => s.Name == "Ca chiều");

            if (shiftSang == null || shiftChieu == null) return;

            var today = VietnamTimeHelper.Now;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);

            var schedules = new List<WorkSchedule>();

            foreach (var user in users)
            {
                for (int day = 0; day < daysInMonth; day++)
                {
                    var currentDate = startDate.AddDays(day);
                    if (currentDate.DayOfWeek == DayOfWeek.Sunday) continue; // Nghỉ chủ nhật

                    // Random ca sáng hoặc chiều
                    // Logic: ID chẵn làm sáng, ID lẻ làm chiều (để đều)
                    var selectedShift = (user.Id % 2 == 0) ? shiftSang : shiftChieu;

                    schedules.Add(new WorkSchedule
                    {
                        UserId = user.Id,
                        ShiftId = selectedShift.Id,
                        WorkDate = currentDate,
                        Status = "Assigned",
                        Note = "Lịch tự động"
                    });
                }
            }
            await context.WorkSchedules.AddRangeAsync(schedules);
            await context.SaveChangesAsync();
        }

        // 3. Hàm tạo dữ liệu chấm công (Dựa trên Location Khu E của bạn)
        public static async Task SeedAttendanceAsync(AppDbContext context)
        {
            if (await context.Attendances.AnyAsync()) return;

            // Lấy các lịch làm việc trong quá khứ của tháng này
            var schedules = await context.WorkSchedules
                .Include(ws => ws.Shift)
                .Where(ws => ws.WorkDate < VietnamTimeHelper.Now.Date)
                .ToListAsync();

            // Lấy địa điểm Khu E để làm địa điểm check-in
            var locationKhuE = await context.Locations.FirstOrDefaultAsync(l => l.Name == "Khu E");
            if (locationKhuE == null) return;

            var random = new Random();
            var attendances = new List<Attendance>();

            foreach (var schedule in schedules)
            {
                // 10% nghỉ không phép
                if (random.Next(0, 100) < 10) continue;

                var shiftStart = schedule.WorkDate.Add(schedule.Shift.StartTime);
                var shiftEnd = schedule.WorkDate.Add(schedule.Shift.EndTime);

                // Giả lập thời gian
                DateTime checkInTime;
                AttendanceStatus status = AttendanceStatus.OnTime;

                // 80% đúng giờ, 20% đi muộn
                if (random.Next(0, 100) < 80)
                    checkInTime = shiftStart.AddMinutes(-random.Next(0, 15)); // Sớm 0-15p
                else
                {
                    checkInTime = shiftStart.AddMinutes(random.Next(1, 45)); // Muộn 1-45p
                    status = AttendanceStatus.Late;
                }

                DateTime checkOutTime = shiftEnd.AddMinutes(random.Next(0, 10)); // Về trễ xíu

                var att = new Attendance
                {
                    UserId = schedule.UserId,
                    WorkScheduleId = schedule.Id,
                    CheckIn = checkInTime,
                    CheckOut = checkOutTime,
                    Status = status,
                    AttendanceType = AttendanceType.GPS,
                    LocationId = locationKhuE.Id,
                    LocationName = locationKhuE.Name,
                    CheckInLatitude = locationKhuE.Latitude,
                    CheckInLongitude = locationKhuE.Longitude,
                    CheckOutLatitude = locationKhuE.Latitude,
                    CheckOutLongitude = locationKhuE.Longitude,
                    DeviceInfo = "IPhone 12 Pro Max"
                };

                attendances.Add(att);
                schedule.Status = "Worked"; // Cập nhật lại lịch
            }

            await context.Attendances.AddRangeAsync(attendances);
            await context.SaveChangesAsync();
        }
    }
}