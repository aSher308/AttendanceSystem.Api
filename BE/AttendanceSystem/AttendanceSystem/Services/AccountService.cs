using AttendanceSystem.Data;
using AttendanceSystem.Models;
using AttendanceSystem.Services.Interfaces;
using AttendanceSystem.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using AttendanceSystem.Helpers;

namespace AttendanceSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;
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
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsEmailConfirmed = false,
                EmailConfirmationToken = Guid.NewGuid().ToString()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var link = $"{confirmUrl}?token={user.EmailConfirmationToken}";
            await SendEmailAsync(user.Email, "Xác nhận email", $"Nhấn vào đây để xác nhận: {link}");

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
            if (user == null || !user.IsEmailConfirmed) return null;
            if (!PasswordHasher.Verify(password, user.PasswordHash)) return null;
            return user;
        }

        public Task LogoutAsync()
        {
            return Task.CompletedTask; // Xử lý session/cookie ở controller
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
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            await SendEmailAsync(
                user.Email,
                "Mã đặt lại mật khẩu",
                $"Mã đặt lại mật khẩu của bạn là:\n\n{user.PasswordResetToken}\n\nMã này sẽ hết hạn sau 1 giờ."
            );
            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            Console.WriteLine("🔁 ResetPasswordAsync được gọi");
            Console.WriteLine("📨 Token nhận được: " + token);

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == token &&
                u.ResetTokenExpiry >= DateTime.UtcNow);

            if (user == null)
            {
                Console.WriteLine("❌ Không tìm thấy user hợp lệ cho token");
                return false;
            }

            Console.WriteLine("✅ Tìm thấy user: " + user.Email);
            Console.WriteLine("📅 Token hết hạn: " + user.ResetTokenExpiry);

            user.PasswordHash = PasswordHasher.Hash(newPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();
            Console.WriteLine("🔒 Mật khẩu đã được đặt lại thành công");

            return true;
        }

        public async Task<bool> IsAdminAsync(int userId)
        {
            return await _context.UserRoles
                .Include(ur => ur.Role)
                .AnyAsync(ur => ur.UserId == userId && ur.Role.Name == "Admin");
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            var from = "manhcuucon@gmail.com";
            var password = "bwtk pazj cvle yflv"; // 🔒 là mã App Password bạn tạo từ Gmail

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(from, password),
                EnableSsl = true
            };

            var mail = new MailMessage(from, to, subject, body)
            {
                IsBodyHtml = false
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
