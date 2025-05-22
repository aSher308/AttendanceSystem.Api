using AttendanceSystem.DTOs;
using AttendanceSystem.Services.Interfaces;
using AttendanceSystem.Attributes; // Giả sử bạn có các attribute RequireUser, RequireAdmin
using Microsoft.AspNetCore.Mvc;

namespace AttendanceSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequireRole("User", "Admin")]
    public class WorkScheduleController : ControllerBase
    {
        private readonly IWorkScheduleService _workScheduleService;
        private readonly IAccountService _accountService;

        public WorkScheduleController(IWorkScheduleService workScheduleService, IAccountService accountService)
        {
            _workScheduleService = workScheduleService;
            _accountService = accountService;
        }

        // Lấy tất cả lịch làm (User chỉ xem được của mình, Admin xem tất cả)
        [HttpGet]
        [RequireRole("User", "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int? id, [FromQuery] int? userId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
                return Unauthorized("Chưa đăng nhập");

            bool isAdmin = await _accountService.IsAdminAsync(currentUserId.Value);

            if (!isAdmin)
            {
                userId = currentUserId.Value;
            }

            var result = await _workScheduleService.GetAllAsync(id, userId, fromDate, toDate);
            return Ok(result);
        }

        // Tạo lịch làm (Chỉ Admin)
        [HttpPost]
        [RequireRole("Admin")]
        public async Task<IActionResult> Create([FromBody] WorkScheduleCreateRequest request)
        {
            var created = await _workScheduleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
        }

        // Sửa lịch làm (Chỉ Admin)
        [HttpPut]
        [RequireRole("Admin")]
        public async Task<IActionResult> Update([FromBody] WorkScheduleUpdateRequest request)
        {
            var result = await _workScheduleService.UpdateAsync(request);
            return result ? Ok("Cập nhật lịch làm thành công") : NotFound();
        }

        // Xóa lịch làm (Chỉ Admin)
        [HttpDelete("{id}")]
        [RequireRole("Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _workScheduleService.DeleteAsync(id);
            return result ? Ok("Xóa lịch làm thành công") : NotFound();
        }
    }
}
