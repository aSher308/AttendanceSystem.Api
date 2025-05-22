using AttendanceSystem.DTOs;
using AttendanceSystem.Helpers;
using AttendanceSystem.Services.Interfaces;
using AttendanceSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AttendanceSystem.Attributes;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly AppDbContext _context;

        public AccountController(IAccountService accountService, AppDbContext context)
        {
            _accountService = accountService;
            _context = context;
        }

        // Đăng ký
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var confirmUrl = $"{Request.Scheme}://{Request.Host}/api/account/confirm-email";
            var result = await _accountService.RegisterAsync(request, confirmUrl);

            if (result == null)
                return Conflict("Email đã được sử dụng.");

            return Ok("Đăng ký thành công! Vui lòng kiểm tra email để xác nhận.");
        }

        // Xác nhận email
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var success = await _accountService.ConfirmEmailAsync(token);
            return success
                ? Ok("Email đã được xác nhận.")
                : BadRequest("Token không hợp lệ hoặc đã hết hạn.");
        }

        // Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _accountService.FindByEmailAsync(request.Email);

            if (user == null)
                return Unauthorized("Email không tồn tại.");

            if (!user.IsEmailConfirmed)
                return Unauthorized("Email chưa được xác nhận.");

            if (!user.IsActive)
                return Unauthorized("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Sai mật khẩu.");

            // Lưu session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("FullName", user.FullName);

            // Lấy role của user
            var roles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            if (roles.Any())
                HttpContext.Session.SetString("Role", roles.First());

            return Ok(new
            {
                Message = "Đăng nhập thành công",
                user.Id,
                user.FullName,
                Role = roles.FirstOrDefault()
            });
        }

        // Đăng xuất
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok("Đăng xuất thành công.");
        }

        // Đổi mật khẩu
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var result = await _accountService.ChangePasswordAsync(userId.Value, request.OldPassword, request.NewPassword);
            return result
                ? Ok("Đổi mật khẩu thành công.")
                : BadRequest("Mật khẩu cũ không đúng.");
        }

        // Quên mật khẩu
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var resetUrl = $"{Request.Scheme}://{Request.Host}/reset-password";
            var success = await _accountService.ForgotPasswordAsync(email, resetUrl);

            return success
                ? Ok("Đã gửi email đặt lại mật khẩu.")
                : NotFound("Không tìm thấy tài khoản.");
        }

        // Reset mật khẩu
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var success = await _accountService.ResetPasswordAsync(request.Token, request.NewPassword);
            return success
                ? Ok("Đặt lại mật khẩu thành công.")
                : BadRequest("Token không hợp lệ hoặc đã hết hạn.");
        }

        // Kiểm tra tài khoản có phải Admin không
        [HttpGet("is-admin")]
        public async Task<IActionResult> IsAdmin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(userId.Value);
            return Ok(new { isAdmin });
        }

        // (Tuỳ chọn) Lấy thông tin tài khoản hiện tại
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return Unauthorized();

            return Ok(new
            {
                UserId = userId,
                FullName = HttpContext.Session.GetString("FullName"),
                Role = HttpContext.Session.GetString("Role")
            });
        }
    }
}
