using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using AttendanceSystem.DTOs;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Helpers;

namespace AttendanceSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AccountService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<User?> RegisterAsync(DTOs.RegisterRequest request, string confirmUrl)
        {
            var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
            if (exists) return null;

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = PasswordHasher.Hash(request.Password),
                LeaveBalance = 12,
                IsActive = true,
                CreatedAt = VietnamTimeHelper.Now,
                UpdatedAt = VietnamTimeHelper.Now,
                IsEmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var link = $"{confirmUrl}?token={user.EmailConfirmationToken}";
            // Sửa: Dùng _emailService thay vì hàm private
            await _emailService.SendEmailAsync(user.Email, "Xác nhận email", $"Nhấn vào đây để xác nhận: {link}");

            return user;
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token);
            if (user == null) return false;

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !user.IsEmailConfirmed || !user.IsActive)
                return null;

            if (!PasswordHasher.Verify(password, user.PasswordHash))
                return null;

            return user;
        }

        public async Task<User?> FindByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !PasswordHasher.Verify(oldPassword, user.PasswordHash)) return false;

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email, string resetUrlBase)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !user.IsEmailConfirmed) return false;

            user.PasswordResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = VietnamTimeHelper.Now.AddHours(1);
            await _context.SaveChangesAsync();

            // Sửa: Dùng _emailService
            await _emailService.SendEmailAsync(
                user.Email,
                "Mã đặt lại mật khẩu",
                $"Mã đặt lại mật khẩu của bạn là:\n\n{user.PasswordResetToken}\n\nMã này sẽ hết hạn sau 1 giờ."
            );
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == token &&
                u.ResetTokenExpiry >= VietnamTimeHelper.Now);

            if (user == null) return false;

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "Admin");
        }
    }
}