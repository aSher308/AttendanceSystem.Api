using AttendanceSystem.Data;
using AttendanceSystem.DTOs;
using AttendanceSystem.Helpers;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace AttendanceSystem.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserResponse>> GetFilteredAsync(int currentUserId, bool isAdmin, string? keyword, int? departmentId, bool? isActive, string? role)
        {
            var query = _context.Users
                .Include(u => u.Department)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(u => u.Id == currentUserId);

            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(u => u.FullName.Contains(keyword) || u.Email.Contains(keyword) || u.PhoneNumber.Contains(keyword));

            if (departmentId.HasValue)
                query = query.Where(u => u.DepartmentId == departmentId);

            if (isActive.HasValue)
                query = query.Where(u => u.IsActive == isActive);

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Name == role));

            return await query.Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                IsActive = u.IsActive,
                LeaveBalance = u.LeaveBalance,
                AvatarUrl = u.AvatarUrl,
                DepartmentName = u.Department != null ? u.Department.Name : null
            }).ToListAsync();
        }

        public async Task<UserResponse> CreateAsync(UserCreateRequest request)
        {
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = PasswordHasher.Hash(request.Password),
                LeaveBalance = 12,
                IsActive = request.IsActive,
                CreatedAt = VietnamTimeHelper.Now,
                UpdatedAt = VietnamTimeHelper.Now,
                IsEmailConfirmed = true,
                DepartmentId = request.DepartmentId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            int? roleId = request.RoleId;
            if (roleId == null)
            {
                var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                roleId = defaultRole?.Id;
            }

            if (roleId != null)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId.Value });
                await _context.SaveChangesAsync();
            }

            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                LeaveBalance = user.LeaveBalance
            };
        }

        public async Task<bool> UpdateAsync(UserUpdateRequest request)
        {
            var user = await _context.Users.FindAsync(request.Id);
            if (user == null) return false;

            user.FullName = request.FullName;
            user.Email = request.Email ?? user.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.DepartmentId = request.DepartmentId;
            user.IsActive = request.IsActive;
            user.UpdatedAt = VietnamTimeHelper.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeStatusAsync(int userId, bool isActive)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignRoleAsync(int userId, int roleId)
        {
            var exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (exists) return true;
            _context.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForceResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            user.PasswordHash = PasswordHasher.Hash(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _context.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return null;
            return new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                LeaveBalance = user.LeaveBalance,
                AvatarUrl = user.AvatarUrl,
                DepartmentName = user.Department?.Name
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !PasswordHasher.Verify(oldPassword, user.PasswordHash))
                return false;

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAvatarAsync(int userId, IFormFile file)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Kiểm tra file hợp lệ
            if (file == null || file.Length == 0) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return false; // Hoặc trả về thông báo lỗi nếu cần
            }

            // Kiểm tra kích thước file (ví dụ 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return false; // Hoặc thông báo file quá lớn
            }

            // Đảm bảo thư mục lưu trữ tồn tại
            var directoryPath = Path.Combine("wwwroot", "avatars");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // Tạo tên file duy nhất
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(directoryPath, fileName);

            // Lưu file vào thư mục
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Cập nhật đường dẫn avatar vào cơ sở dữ liệu
            user.AvatarUrl = $"/avatars/{fileName}";
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            return await _context.UserRoles.Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "Admin");
        }

        public async Task<int> ImportUsersFromExcelAsync(IFormFile file)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage(file.OpenReadStream());
            var sheet = package.Workbook.Worksheets.First();

            int count = 0;
            for (int row = 2; row <= sheet.Dimension.End.Row; row++)
            {
                var fullName = sheet.Cells[row, 1].Text?.Trim();
                var email = sheet.Cells[row, 2].Text?.Trim();
                var password = sheet.Cells[row, 3].Text?.Trim();
                var phone = sheet.Cells[row, 4].Text?.Trim();
                var roleName = sheet.Cells[row, 5].Text?.Trim();
                var departmentId = int.TryParse(sheet.Cells[row, 6].Text, out var deptId) ? deptId : (int?)null;

                if (string.IsNullOrEmpty(email) || await _context.Users.AnyAsync(u => u.Email == email))
                    continue;

                var user = new User
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phone,
                    PasswordHash = PasswordHasher.Hash(password ?? "123456"),
                    LeaveBalance = 12,
                    IsActive = true,
                    CreatedAt = VietnamTimeHelper.Now,
                    UpdatedAt = VietnamTimeHelper.Now,
                    DepartmentId = departmentId
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(roleName))
                {
                    var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                    if (role != null)
                    {
                        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
                        await _context.SaveChangesAsync();
                    }
                }

                count++;
            }
            return count;
        }
        public async Task<List<string>> GetRolesOfUser(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> RemoveRoleAsync(int userId, int roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (userRole == null) return false;

            _context.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
