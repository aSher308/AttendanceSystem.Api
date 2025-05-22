using AttendanceSystem.DTOs;
using AttendanceSystem.Helpers;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        // Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var confirmUrl = $"{Request.Scheme}://{Request.Host}/api/account/confirm-email";
            var result = await _accountService.RegisterAsync(request, confirmUrl);
            if (result == null) return Conflict("Email already exists");
            return Ok("Đăng ký thành công! Vui lòng kiểm tra email để xác nhận.");
        }
        // ConfirmEmail để được phép đăng nhập
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            var success = await _accountService.ConfirmEmailAsync(token);
            return success ? Ok("Email đã được xác nhận.") : BadRequest("Token không hợp lệ hoặc đã hết hạn.");
        }
        // Login 
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _accountService.FindByEmailAsync(request.Email);

            if (user == null)
                return Unauthorized("Email không tồn tại.");

            if (!user.IsEmailConfirmed)
                return Unauthorized("Email chưa được xác nhận.");

            if (!user.IsActive)
                return Unauthorized("Tài khoản của bạn đã bị khoá. Vui lòng liên hệ quản trị viên.");

            if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
                return Unauthorized("Sai mật khẩu.");

            HttpContext.Session.SetInt32("UserId", user.Id);
            return Ok(new { message = "Đăng nhập thành công", user.Id, user.FullName });
        }
        // Logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            HttpContext.Session.Clear(); 
            return Ok("Đăng xuất thành công");
        }
        // Đổi mật khẩu
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var result = await _accountService.ChangePasswordAsync(userId.Value, request.OldPassword, request.NewPassword);
            return result ? Ok("Đổi mật khẩu thành công") : BadRequest("Mật khẩu cũ không đúng");
        }
        // Quên mật khẩu
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            var resetUrl = $"{Request.Scheme}://{Request.Host}/reset-password"; 
            var success = await _accountService.ForgotPasswordAsync(email, resetUrl);
            return success ? Ok("Đã gửi email đặt lại mật khẩu.") : NotFound("Không tìm thấy tài khoản.");
        }
        // Reset mật khẩu
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var success = await _accountService.ResetPasswordAsync(request.Token, request.NewPassword);
            return success ? Ok("Đặt lại mật khẩu thành công.") : BadRequest("Token không hợp lệ hoặc đã hết hạn.");
        }
        // Check Admin
        [HttpGet("is-admin")]
        public async Task<IActionResult> IsAdmin()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var isAdmin = await _accountService.IsAdminAsync(userId.Value);
            return Ok(new { isAdmin });
        }
    }
}
