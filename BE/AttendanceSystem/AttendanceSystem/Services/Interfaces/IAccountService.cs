using AttendanceSystem.Models;
using AttendanceSystem.DTOs;
using Microsoft.AspNetCore.Identity.Data;
namespace AttendanceSystem.Services.Interfaces
{
    public interface IAccountService
    {
        // Đăng ký tài khoản mới + gửi email xác nhận
        Task<User?> RegisterAsync(DTOs.RegisterRequest request, string confirmUrl);

        // Xác nhận email qua token
        Task<bool> ConfirmEmailAsync(string token);

        // Đăng nhập
        Task<User?> LoginAsync(string email, string password);

        // Hàm riêng để lấy user chưa kiểm tra mật khẩu
        Task<User?> FindByEmailAsync(string email);

        // Đăng xuất
        Task LogoutAsync();

        // Đổi mật khẩu khi đã đăng nhập
 
        Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);

        // Gửi email đặt lại mật khẩu (forgot password)
        Task<bool> ForgotPasswordAsync(string email, string resetUrlBase);

        // Đặt lại mật khẩu (reset password bằng token)
        Task<bool> ResetPasswordAsync(string token, string newPassword);

        // Kiểm tra người dùng có phải admin không
        Task<bool> IsAdminAsync(int userId);
    }
}
