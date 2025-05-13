using AttendanceSystem.Attributes;
using AttendanceSystem.DTOs;
using AttendanceSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Lấy danh sách người dùng có bộ lọc
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] int? departmentId,
                                                [FromQuery] bool? isActive, [FromQuery] string? role)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var isAdmin = await _userService.IsAdminAsync(userId.Value);
            var users = await _userService.GetFilteredAsync(userId.Value, isAdmin, keyword, departmentId, isActive, role);
            return Ok(users);
        }

        // Tạo user mới
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] UserCreateRequest request)
        {
            var created = await _userService.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        // Cập nhật user
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserUpdateRequest request)
        {
            var result = await _userService.UpdateAsync(request);
            return result ? Ok("Cập nhật thành công") : NotFound();
        }

        // Đổi trạng thái hoạt động
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromQuery] bool isActive)
        {
            var result = await _userService.ChangeStatusAsync(id, isActive);
            return result ? Ok("Đã cập nhật trạng thái") : NotFound();
        }

        // Gán vai trò cho user
        [HttpPost("{id}/assign-role")]
        public async Task<IActionResult> AssignRole(int id, [FromBody] int roleId)
        {
            var result = await _userService.AssignRoleAsync(id, roleId);
            return result ? Ok("Đã gán quyền") : BadRequest("Gán quyền thất bại");
        }

        // Đặt lại mật khẩu
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPasswordByAdmin(int id, [FromBody] string newPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var isAdmin = await _userService.IsAdminAsync(userId.Value);
            if (!isAdmin) return Forbid();

            var result = await _userService.ForceResetPasswordAsync(id, newPassword);
            return result ? Ok("Đã đặt lại mật khẩu") : NotFound();
        }

        // Xem + Cập nhật thông tin cá nhân
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var user = await _userService.GetByIdAsync(userId.Value);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UserUpdateRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || request.Id != userId.Value) return Unauthorized();
            var result = await _userService.UpdateAsync(request);
            return result ? Ok("Cập nhật thông tin cá nhân") : NotFound();
        }

        // Đổi mật khẩu
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var result = await _userService.ChangePasswordAsync(userId.Value, request.OldPassword, request.NewPassword);
            return result ? Ok("Đổi mật khẩu thành công") : BadRequest("Sai mật khẩu cũ");
        }
/*
        // Cập nhật ảnh khuôn mặt
        [HttpPost("me/avatar")]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();
            var result = await _userService.UpdateAvatarAsync(userId.Value, file);
            return result ? Ok("Đã cập nhật avatar") : BadRequest("Cập nhật thất bại");
        }*/

        // Import Excel danh sách user
        [HttpPost("import")]
        public async Task<IActionResult> ImportUsers(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File không hợp lệ");
            var result = await _userService.ImportUsersFromExcelAsync(file);
            return Ok(new { message = $"Import {result} người dùng thành công" });
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportUsers()
        {
            var users = await _userService.GetFilteredAsync(0, true, null, null, null, null); // Lấy tất cả

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Users");

            // Header
            sheet.Cells[1, 1].Value = "Full Name";
            sheet.Cells[1, 2].Value = "Email";
            sheet.Cells[1, 3].Value = "Phone";
            sheet.Cells[1, 4].Value = "Role";
            sheet.Cells[1, 5].Value = "Department";
            sheet.Cells[1, 6].Value = "Active";

            int row = 2;
            foreach (var user in users)
            {
                sheet.Cells[row, 1].Value = user.FullName;
                sheet.Cells[row, 2].Value = user.Email;
                sheet.Cells[row, 3].Value = user.PhoneNumber;
                sheet.Cells[row, 4].Value = string.Join(",", await _userService.GetRolesOfUser(user.Id));
                sheet.Cells[row, 5].Value = user.DepartmentName;
                sheet.Cells[row, 6].Value = user.IsActive ? "Yes" : "No";
                row++;
            }

            var stream = new MemoryStream(package.GetAsByteArray());
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "users.xlsx");
        }

    }
}
